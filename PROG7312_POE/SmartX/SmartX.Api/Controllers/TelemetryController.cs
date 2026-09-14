using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryService _telemetryService;

        public TelemetryController(TelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        [HttpPost("temperature")]
        public ActionResult<TelemetryPacket<double>> AddTemperature(
            string deviceIdentifier,
            double value)
        {
            // Temperature readings use double because they can have decimals.
            var packet = _telemetryService.CreatePacket(
                deviceIdentifier,
                value);

            return Ok(packet);
        }

        [HttpPost("switch")]
        public ActionResult<TelemetryPacket<bool>> AddSwitchStatus(
            string deviceIdentifier,
            bool value)
        {
            // A switch only needs true or false.
            var packet = _telemetryService.CreatePacket(
                deviceIdentifier,
                value);

            return Ok(packet);
        }

        [HttpPost("average")]
        public ActionResult<double> GetAverage(double[] readings)
        {
            var average = _telemetryService.GetAverageReading(readings);

            return Ok(average);
        }

        [HttpPost("combine")]
        public ActionResult<TelemetryReading> CombineReadings(
    TelemetryReading firstReading,
    TelemetryReading secondReading)
        {
            // Uses the overloaded + operator from TelemetryReading.
            var combined = _telemetryService.CombineReadings(
                firstReading,
                secondReading);

            return Ok(combined);
        }
    }
}