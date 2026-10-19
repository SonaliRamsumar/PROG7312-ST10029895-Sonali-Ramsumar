
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
        private readonly SensorService _sensorService;

        public FilesController(
            FileUploadService fileUploadService,
            SensorService sensorService)
        {
            _fileUploadService = fileUploadService;
            _sensorService = sensorService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(
            IFormFile file)
        {
            // Check that a file was selected.
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a file first."
                });
            }

            // Limit uploads to 10 MB.
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    message = "File is too large. Maximum size is 10 MB."
                });
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

            var extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "This file type is not allowed."
                });
            }

            try
            {
                // Save the original file first.
                var savedFile =
                    await _fileUploadService.SaveFileAsync(file);

                var registeredSensors =
                    new List<SensorRegistration>();

                var errors = new List<string>();

                // Only text files are processed as device lists.
                if (extension == ".txt")
                {
                    var lines = await System.IO.File.ReadAllLinesAsync(
                        savedFile.FilePath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var lineNumber = i + 1;
                        var line = lines[i].Trim();

                        // Ignore blank lines and comments.
                        if (string.IsNullOrWhiteSpace(line) ||
                            line.StartsWith("#"))
                        {
                            continue;
                        }

                        // Accept either pipe-separated or comma-separated rows.
                        var separator = line.Contains('|') ? '|' : ',';
                        var fields = line.Split(separator);

                        // Skip the optional header row.
                        if (fields.Length > 0 &&
                            fields[0].Trim().Equals(
                                "DeviceIdentifier",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Expected order:
                        // DeviceIdentifier, SensorName,
                        // DeploymentLocation, Category.
                        if (fields.Length != 4)
                        {
                            errors.Add(
                                $"Line {lineNumber}: Expected four fields.");
                            continue;
                        }

                        var deviceIdentifier = fields[0].Trim();
                        var sensorName = fields[1].Trim();
                        var deploymentLocation = fields[2].Trim();
                        var category = fields[3].Trim();

                        // Make sure all required fields are provided.
                        if (string.IsNullOrWhiteSpace(deviceIdentifier) ||
                            string.IsNullOrWhiteSpace(sensorName) ||
                            string.IsNullOrWhiteSpace(deploymentLocation) ||
                            string.IsNullOrWhiteSpace(category))
                        {
                            errors.Add(
                                $"Line {lineNumber}: One or more fields are empty.");
                            continue;
                        }

                        // Do not register a device that already exists.
                        if (_sensorService.DeviceExists(deviceIdentifier))
                        {
                            errors.Add(
                                $"Line {lineNumber}: Device '{deviceIdentifier}' is already registered.");
                            continue;
                        }

                        var sensor = new SensorRegistration
                        {
                            DeviceIdentifier = deviceIdentifier,
                            SensorName = sensorName,
                            DeploymentLocation = deploymentLocation,
                            Category = category
                        };

                        try
                        {
                            // Add the device to the existing sensor collection.
                            var registeredSensor =
                                _sensorService.Add(sensor);

                            registeredSensors.Add(registeredSensor);
                        }
                        catch (InvalidOperationException)
                        {
                            errors.Add(
                                $"Line {lineNumber}: Device '{deviceIdentifier}' could not be registered. It may already exist.");
                        }
                    }
                }

                var message = extension != ".txt"
                    ? "File uploaded successfully. Device registration is only performed for .txt files."
                    : $"File uploaded successfully. {registeredSensors.Count} device(s) registered.";

                return Ok(new
                {
                    message,
                    fileName = savedFile.FileName,
                    uploadedAt = savedFile.UploadedAt,
                    registeredSensors,
                    errors,
                    skippedLines = errors.Count
                });
            }
            catch (IOException)
            {
                return StatusCode(500, new
                {
                    message = "The file could not be saved or read."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(500, new
                {
                    message = "The server does not have permission to access the upload folder."
                });
            }
        }
    }
}
