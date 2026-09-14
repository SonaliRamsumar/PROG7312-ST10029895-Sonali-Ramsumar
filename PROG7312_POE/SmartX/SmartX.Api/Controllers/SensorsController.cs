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
        public ActionResult<SensorRegistration> AddSensor(SensorRegistration sensor)
        {
            // Checks that the user entered one of our allowed locations.
            if (!_locationValidationService.IsValidLocation(sensor.DeploymentLocation))
            {
                return BadRequest("Invalid deployment location.");
            }

            var createdSensor = _sensorService.Add(sensor);

            return Ok(createdSensor);
        }
    }
}