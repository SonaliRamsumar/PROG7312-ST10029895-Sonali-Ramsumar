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
            // Makes sure the user actually selected a file.
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was selected.");
            }

            var savedFile = await _fileUploadService.SaveFileAsync(file);

            return Ok(savedFile);
        }
    }
}