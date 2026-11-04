using Microsoft.AspNetCore.Http;

namespace ServiceAbstraction
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the uploaded file to the specified directory and returns the saved file URL.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="directoryPath">The directory path where the file should be saved.</param>
        /// <param name="maxSize">The maximum allowed file size in bytes.</param>
        /// <returns>The relative file URL.</returns>
        Task<string> SaveFileAsync(IFormFile file, string directoryPath, int maxSize);

        /// <summary>
        /// Deletes the specified file from storage.
        /// </summary>
        /// <param name="fileUrl">The relative or absolute path of the file.</param>
        Task DeleteFileAsync(string fileUrl);
    }
}
