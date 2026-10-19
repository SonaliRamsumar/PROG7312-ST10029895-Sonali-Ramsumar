
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

            // Create the upload folder if it does not exist.
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<SensorFile> SaveFileAsync(IFormFile file)
        {
            var originalFileName = Path.GetFileName(file.FileName);

            // Give each uploaded file a unique name.
            var uniqueFileName =
                $"{Guid.NewGuid()}_{originalFileName}";

            var filePath = Path.Combine(
                _uploadFolder,
                uniqueFileName);

            // Save the uploaded file asynchronously.
            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

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
