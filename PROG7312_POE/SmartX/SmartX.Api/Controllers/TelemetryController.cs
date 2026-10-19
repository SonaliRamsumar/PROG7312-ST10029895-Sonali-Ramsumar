
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
        private readonly TelemetryQueue<double> _temperatureQueue;

        public TelemetryController(
            TelemetryService telemetryService,
            TelemetryHistoryService historyService,
            TelemetryQueue<double> temperatureQueue)
        {
            _telemetryService = telemetryService;
            _historyService = historyService;
            _temperatureQueue = temperatureQueue;
        }

        [HttpPost("temperature")]
        public ActionResult<TelemetryPacket<double>> AddTemperature(
            string deviceIdentifier,
            double value)
        {
            var packet = _telemetryService.CreatePacket(
                deviceIdentifier,
                value);

            // Store the reading in history and queue it for processing.
            _historyService.AddReading(packet.Timestamp, packet.Value);
            _temperatureQueue.Enqueue(packet);

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
            // Converts historical batches into one list for processing.
            var readings = _historyService
                .ConvertHistoricalBatchesToList(batches);

            return Ok(readings);
        }

        [HttpGet("history/readings")]
        public IActionResult GetStoredReadings()
        {
            // Returns numeric readings from oldest to newest.
            var readings = _historyService
                .GetReadingsInChronologicalOrder();

            var result = readings.Select(reading => new
            {
                Timestamp = reading.Timestamp,
                Value = reading.Value
            }).ToList();

            return Ok(result);
        }

        [HttpGet("queue/pending")]
        public IActionResult GetPendingReadings()
        {
            // Shows queued readings without removing them.
            return Ok(_temperatureQueue.GetPendingPackets());
        }

        [HttpPost("queue/process-next")]
        public IActionResult ProcessNextReading()
        {
            // Removes the oldest waiting reading first.
            if (!_temperatureQueue.TryDequeue(out var packet)
                || packet == null)
            {
                return NotFound(new
                {
                    Message = "There are no pending temperature readings."
                });
            }

            return Ok(packet);
        }
    }
}
