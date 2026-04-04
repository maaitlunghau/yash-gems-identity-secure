using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace YashGems.Identity.Application.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletionResultAsync(string publicId);
}
