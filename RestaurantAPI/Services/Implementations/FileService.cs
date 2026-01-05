using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileService> _logger;
    private readonly IWebHostEnvironment _environment;

    public FileService(
        IConfiguration configuration,
        ILogger<FileService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

    public async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File không hợp lệ", null);
            }

            // Validate file
            if (!IsValidImageFile(file))
            {
                return (false, "File không phải là hình ảnh hợp lệ", null);
            }

            // Get max file size from config (default 5MB)
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880);
            if (file.Length > maxFileSize)
            {
                var maxSizeMB = maxFileSize / 1024 / 1024;
                return (false, $"Kích thước file vượt quá {maxSizeMB}MB", null);
            }

            // Create upload directory if not exists
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            var relativePath = $"/uploads/{folder}/{fileName}";

            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);
            return (true, "Upload file thành công", relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return (false, "Đã xảy ra lỗi khi upload file", null);
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // Convert relative path to physical path
            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(physicalPath))
            {
                await Task.Run(() => File.Delete(physicalPath));
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            return false;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null)
            return false;

        // Get allowed extensions from config
        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return false;

        // Check MIME type
        var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }
}