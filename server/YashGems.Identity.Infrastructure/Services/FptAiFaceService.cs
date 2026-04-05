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
        var fptSettings = _configuration.GetSection("FptAiSettings");
        var apiKey = fptSettings["ApiKey"];
        var url = fptSettings["BaseUrl"];

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(facePhotoUrl), "image_url1");
        content.Add(new StringContent(idCardFrontUrl), "image_url2");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var jsonString = await response.Content.ReadAsStringAsync();

            // Phân tích kết quả JSON trả về từ FPT AI
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            // FPT trả về: { "data": { "similarity": 85.5 }, "errorCode": 0 }
            if (root.GetProperty("errorCode").GetInt32() == 0)
            {
                var similarity = root.GetProperty("data").GetProperty("similarity").GetDouble();
                return similarity;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> LỖI FPT AI: {ex.Message}");
            return 0;
        }
    }
}
