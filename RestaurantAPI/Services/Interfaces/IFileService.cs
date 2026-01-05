namespace RestaurantAPI.Services.Interfaces;

public interface IFileService
{
    Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    bool IsValidImageFile(IFormFile file);
}