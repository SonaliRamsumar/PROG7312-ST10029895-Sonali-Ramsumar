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

        public SensorsController(SensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpGet]
        public ActionResult<List<SensorRegistration>> GetSensors()
        {
            return Ok(_sensorService.GetAll());
        }

        [HttpPost]
        public ActionResult<SensorRegistration> AddSensor(SensorRegistration sensor)
        {
            var createdSensor = _sensorService.Add(sensor);
            return Ok(createdSensor);
        }
    }
}