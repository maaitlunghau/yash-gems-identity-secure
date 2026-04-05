using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YashGems.Identity.Application.Interfaces;

namespace YashGems.Identity.Infrastructure.Services;

public class FptAiFaceService : IAiFaceService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public FptAiFaceService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<double> CompareFacesAsync(string facePhotoUrl, string idCardFrontUrl)
    {
        var fptSettings = _configuration.GetSection("Key");
        var apiKey = fptSettings["ApiKey"];
        var url = fptSettings["BaseUrl"];

        // 1. CHUẨN BỊ REQUEST
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", apiKey);

        // 2. TẢI ẢNH TỪ CLOUDINARY VỀ BACKEND (ĐẢM BẢO DỮ LIỆU CHUẨN)
        var facePhotoBytes = await _httpClient.GetByteArrayAsync(facePhotoUrl);
        var idCardBytes = await _httpClient.GetByteArrayAsync(idCardFrontUrl);

        // 3. ĐÓNG GÓI MULTIPART VỚI TÊN TRƯỜNG LÀ "file[]" (THEO CÚ PHÁP FPT AI)
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(facePhotoBytes), "file[]", "face.jpg");
        content.Add(new ByteArrayContent(idCardBytes), "file[]", "idcard.jpg");
        request.Content = content;

        try
        {
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"--> FPT AI RAW RESPONSE: {jsonString}");

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            // 4. KIỂM TRA MÃ TRẢ VỀ (Lúc này có thể là field "code" hoặc "errorCode")
            if (root.TryGetProperty("code", out var code) && code.GetString() == "200")
            {
                // FPT trả về similarity bên trong object data (Dạng String hoặc Number)
                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("similarity", out var sim))
                {
                    // Thử lấy giá trị Double (FPT hay trả về dạng số hoặc string số)
                    return double.Parse(sim.ToString());
                }
            }
            // Nếu theo cấu trúc Vision cũ
            else if (root.TryGetProperty("errorCode", out var errCode) && errCode.GetInt32() == 0)
            {
                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("similarity", out var sim))
                {
                    return double.Parse(sim.ToString(), CultureInfo.InvariantCulture);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> LỖI XỬ LÝ AI: {ex.Message}");
            return 0;
        }
    }
}
