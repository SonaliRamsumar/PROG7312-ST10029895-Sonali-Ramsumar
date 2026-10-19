
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
        private readonly LiveTelemetryService _liveTelemetryService;

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
            _liveTelemetryService = liveTelemetryService;
        }

        [HttpPost("temperature")]
        public ActionResult<TelemetryPacket<double>> AddTemperature(
            string deviceIdentifier,
            double value)
        {
            // Create a timestamped packet for the incoming reading.
            var packet = _telemetryService.CreatePacket(
                deviceIdentifier,
                value);

            // Record the reading in the existing history and FIFO queue.
            _historyService.AddReading(packet.Timestamp, packet.Value);
            _temperatureQueue.Enqueue(packet);

            // Retain the device identifier for the live dashboard.
            _liveTelemetryService.AddReading(packet);

            string severity = GetTemperatureSeverity(packet.Value);

            // Only warning and critical readings create alerts.
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

        // Supplies recent packets, including their source device identifiers.
        [HttpGet("live/readings")]
        public IActionResult GetLiveReadings()
        {
            return Ok(_liveTelemetryService.GetRecentReadings());
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
            // Convert historical batches into one list for processing.
            var readings = _historyService
                .ConvertHistoricalBatchesToList(batches);

            return Ok(readings);
        }

        [HttpGet("history/readings")]
        public IActionResult GetStoredReadings()
        {
            // Return numeric readings from oldest to newest.
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
            // Show queued readings without removing them.
            return Ok(_temperatureQueue.GetPendingPackets());
        }

        [HttpPost("queue/process-next")]
        public IActionResult ProcessNextReading()
        {
            // Remove the oldest waiting reading first.
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
            // Return alerts with the most urgent ones first.
            return Ok(_alertQueue.GetPendingAlertsInPriorityOrder());
        }

        [HttpPost("alerts/process-next")]
        public IActionResult ProcessNextAlert()
        {
            // Remove the most urgent alert from the priority queue.
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
