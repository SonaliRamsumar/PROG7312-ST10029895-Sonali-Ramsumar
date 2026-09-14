using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly SensorService _sensorService;
        private readonly LocationValidationService _locationValidationService;

        public SensorsController(
            SensorService sensorService,
            LocationValidationService locationValidationService)
        {
            _sensorService = sensorService;
            _locationValidationService = locationValidationService;
        }

        [HttpGet]
        public ActionResult<List<SensorRegistration>> GetSensors()
        {
            return Ok(_sensorService.GetAll());
        }

        [HttpPost]
        public ActionResult<SensorRegistration> AddSensor(
            SensorRegistration sensor)
        {
            // Makes sure the deployment location is one of the allowed locations.
            if (!_locationValidationService.IsValidLocation(
                sensor.DeploymentLocation))
            {
                return BadRequest("Invalid deployment location.");
            }

            // Stops the same device identifier from being registered twice.
            if (_sensorService.DeviceExists(sensor.DeviceIdentifier))
            {
                return BadRequest(
                    "A sensor with this device identifier already exists.");
            }

            var createdSensor = _sensorService.Add(sensor);

            return Ok(createdSensor);
        }
    }
}