using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using YashGems.Identity.Application.Interfaces;

namespace YashGems.Identity.Infrastructure.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IConfiguration configuration)
    {
        var acc = new Account(
            configuration["CloudinarySettings:CloudName"],
            configuration["CloudinarySettings:ApiKey"],
            configuration["CloudinarySettings:ApiSecret"]
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Width(1600)
                    .Crop("limit")
                    .Quality("auto")
                    .FetchFormat("auto"),
                Folder = "yash-gems-ekyc"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                Console.WriteLine($"--> CLOUDINARY DEBUG: StatusCode: {uploadResult.StatusCode}");
                Console.WriteLine($"--> CLOUDINARY DEBUG: Error Message: {uploadResult.Error.Message}");
            }
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeletionResultAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
