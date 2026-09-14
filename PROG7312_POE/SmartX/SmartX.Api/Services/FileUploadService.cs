using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class FileUploadService
    {
        private readonly string _uploadFolder;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _uploadFolder = Path.Combine(environment.ContentRootPath, "Uploads");

            // Makes sure the Uploads folder exists before we save files.
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<SensorFile> SaveFileAsync(IFormFile file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_uploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return new SensorFile
            {
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };
        }
    }
}