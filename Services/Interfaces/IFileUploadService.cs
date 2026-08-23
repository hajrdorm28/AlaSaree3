using Microsoft.AspNetCore.Http;

namespace AlaSaree3.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadProductImageAsync(IFormFile file);
        void DeleteFile(string? relativeFilePath);
    }
}
