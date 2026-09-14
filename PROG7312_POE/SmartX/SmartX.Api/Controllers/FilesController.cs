using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;

        public FilesController(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<SensorFile>> UploadFile(IFormFile file)
        {
            // Makes sure a file was actually selected.
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was selected.");
            }

            // Keeps uploads small enough for this project.
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File is too large. Maximum size is 10 MB.");
            }

            var allowedExtensions = new[]
            {
                ".txt",
                ".log",
                ".json",
                ".pdf",
                ".png",
                ".jpg",
                ".jpeg"
            };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("This file type is not allowed.");
            }

            var savedFile = await _fileUploadService.SaveFileAsync(file);

            return Ok(savedFile);
        }
    }
}