
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
        private readonly AlertPriorityQueue _alertQueue;

        public TelemetryController(
            TelemetryService telemetryService,
            TelemetryHistoryService historyService,
            TelemetryQueue<double> temperatureQueue,
            AlertPriorityQueue alertQueue,
            LiveTelemetryService liveTelemetryService)
        {
            _telemetryService = telemetryService;
            _historyService = historyService;
            _temperatureQueue = temperatureQueue;
            _alertQueue = alertQueue;
        }

        [HttpPost("temperature")]
        public ActionResult<TelemetryPacket<double>> AddTemperature(
            string deviceIdentifier,
            double value)
        {
            var packet = _telemetryService.CreatePacket(
                deviceIdentifier,
                value);

            _historyService.AddReading(packet);

            _temperatureQueue.Enqueue(packet);

            string severity = GetTemperatureSeverity(packet.Value);

            if (severity != "Normal")
            {
                var alert = new TelemetryAlert
                {
                    DeviceIdentifier = packet.DeviceIdentifier,
                    Value = packet.Value,
                    Severity = severity,
                    Timestamp = packet.Timestamp,
                    Message = severity == "Critical"
                        ? $"Critical temperature: {packet.Value} degrees"
                        : $"Temperature warning: {packet.Value} degrees"
                };

                _alertQueue.EnqueueAlert(alert);
            }

            return Ok(packet);
        }

        // Returns historical readings to the dashboard.
        [HttpGet("live/readings")]
        public IActionResult GetLiveReadings()
        {
            return Ok(
                _historyService.GetReadingsInChronologicalOrder());
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
            var readings = _historyService
                .ConvertHistoricalBatchesToList(batches);

            return Ok(readings);
        }

        [HttpGet("history/readings")]
        public IActionResult GetStoredReadings()
        {
            return Ok(_historyService.GetReadingsInChronologicalOrder());
        }

        [HttpGet("history/readings/{deviceIdentifier}")]
        public IActionResult GetDeviceReadings(string deviceIdentifier)
        {
            var readings = _historyService
                .GetReadingsForDevice(deviceIdentifier);

            return Ok(readings);
        }

        [HttpGet("history/range")]
        public IActionResult GetReadingsInRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromQuery] string? deviceIdentifier = null)
        {
            try
            {
                var readings = _historyService.GetReadingsInRange(
                    start,
                    end,
                    deviceIdentifier);

                return Ok(readings);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    Message = exception.Message
                });
            }
        }

        [HttpGet("queue/pending")]
        public IActionResult GetPendingReadings()
        {
            return Ok(_temperatureQueue.GetPendingPackets());
        }

        [HttpPost("queue/process-next")]
        public IActionResult ProcessNextReading()
        {
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

        [HttpGet("alerts/pending")]
        public IActionResult GetPendingAlerts()
        {
            return Ok(_alertQueue.GetPendingAlertsInPriorityOrder());
        }

        [HttpPost("alerts/process-next")]
        public IActionResult ProcessNextAlert()
        {
            if (!_alertQueue.TryDequeueAlert(out var alert)
                || alert == null)
            {
                return NotFound(new
                {
                    Message = "There are no pending alerts."
                });
            }

            return Ok(alert);
        }

        private static string GetTemperatureSeverity(double value)
        {
            if (value < 10 || value > 40)
            {
                return "Critical";
            }

            if (value < 15 || value > 35)
            {
                return "Warning";
            }

            return "Normal";
        }
    }
}
