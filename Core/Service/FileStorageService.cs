using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using ServiceAbstraction;
using Shared.SharedResources;

namespace Service
{
    public class FileStorageService(IStringLocalizer<SharedResources> localizer) : IFileStorageService
    {
        private readonly string _rootPath=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        public async Task<string> SaveFileAsync(IFormFile file, string directoryPath,int maxSize)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            if (file.Length > maxSize)
                throw new ArgumentException(String.Format(localizer[SharedResourcesKeys.SizeLimitExceeded],maxSize));

            string fullPath = Path.Combine(_rootPath, directoryPath);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(fullPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = Path.Combine(directoryPath, uniqueFileName).Replace("\\", "/");
            return relativePath;
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return Task.CompletedTask;

            fileUrl = fileUrl.TrimStart('/', '\\');

            string fullPath = Path.Combine(_rootPath, fileUrl);

            fullPath = fullPath.Replace("/", Path.DirectorySeparatorChar.ToString());

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

    }
}
