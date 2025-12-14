using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HRManagement.Recruitment.Api.Application.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder, string[] allowedExtensions, CancellationToken ct = default);
    Task<byte[]?> GetFileAsync(string filePath, CancellationToken ct = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken ct = default);
    string GetFileUrl(string filePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string UploadsFolder = "uploads";
    private const long MaxFileSize = 10 * 1024 * 1024;

    public FileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder, string[] allowedExtensions, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл не предоставлен");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("Размер файла превышает 10 МБ");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException($"Недопустимый формат файла. Разрешены: {string.Join(", ", allowedExtensions)}");

        var uploadsPath = Path.Combine(_environment.ContentRootPath, UploadsFolder, folder);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return Path.Combine(folder, fileName);
    }

    public async Task<byte[]?> GetFileAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, UploadsFolder, filePath);
        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, UploadsFolder, filePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return $"/files/{filePath}";

        return $"{request.Scheme}://{request.Host}/files/{filePath}";
    }
}
