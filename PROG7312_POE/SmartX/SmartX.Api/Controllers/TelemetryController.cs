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
        private readonly TelemetryHistoryService _historyService;

        public TelemetryController(
            TelemetryService telemetryService,
            TelemetryHistoryService historyService)
        {
            _telemetryService = telemetryService;
            _historyService = historyService;
        }

        [HttpPost("temperature")]
        public ActionResult<TelemetryPacket<double>> AddTemperature(
            string deviceIdentifier,
            double value)
        {
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
            CombineTelemetryRequest request)
        {
            var combined = _telemetryService.CombineReadings(
                request.FirstReading,
                request.SecondReading);

            return Ok(combined);
        }

        [HttpPost("history")]
        public ActionResult<List<double>> ProcessHistory(double[][] batches)
        {
            // Converts jagged telemetry batches into one List for easier processing.
            var readings = _historyService
                .ConvertHistoricalBatchesToList(batches);

            return Ok(readings);
        }
    }
}