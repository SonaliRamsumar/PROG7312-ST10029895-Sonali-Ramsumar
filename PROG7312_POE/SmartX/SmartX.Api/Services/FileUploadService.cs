using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class FileUploadService
    {
        private readonly string _uploadFolder;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _uploadFolder = Path.Combine(
                environment.ContentRootPath,
                "Uploads");

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<SensorFile> SaveFileAsync(IFormFile file)
        {
            var originalFileName = Path.GetFileName(file.FileName);

            // Adds a unique value so files with the same name are not overwritten.
            var uniqueFileName =
                $"{Guid.NewGuid()}_{originalFileName}";

            var filePath = Path.Combine(
                _uploadFolder,
                uniqueFileName);

            using var stream = new FileStream(
                filePath,
                FileMode.Create);

            await file.CopyToAsync(stream);

            return new SensorFile
            {
                FileName = originalFileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };
        }
    }
}