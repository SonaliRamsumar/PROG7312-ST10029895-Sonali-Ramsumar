
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryProcessingBackgroundService : BackgroundService
    {
        private readonly TelemetryQueue<double> _telemetryQueue;
        private readonly AlertPriorityQueue _alertQueue;
        private readonly ILogger<TelemetryProcessingBackgroundService> _logger;

        public TelemetryProcessingBackgroundService(
            TelemetryQueue<double> telemetryQueue,
            AlertPriorityQueue alertQueue,
            ILogger<TelemetryProcessingBackgroundService> logger)
        {
            _telemetryQueue = telemetryQueue;
            _alertQueue = alertQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Smart-X background telemetry processing has started.");

            // The service checks for pending work every second.
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    // Process urgent alerts before normal telemetry readings.
                    ProcessPendingAlerts();

                    int processedReadings = 0;
                    const int maximumReadingsPerCycle = 25;

                    // Process readings in FIFO order.
                    // The limit prevents one cycle from running indefinitely
                    // if new readings continue arriving.
                    while (processedReadings < maximumReadingsPerCycle
                           && _telemetryQueue.TryDequeue(out var packet)
                           && packet != null)
                    {
                        _logger.LogInformation(
                            "Processed telemetry from device {DeviceIdentifier}. " +
                            "Value: {Value}, Timestamp: {Timestamp}",
                            packet.DeviceIdentifier,
                            packet.Value,
                            packet.Timestamp);

                        processedReadings++;

                        // Check for newly arrived alerts before processing
                        // the next telemetry reading.
                        ProcessPendingAlerts();
                    }

                    if (processedReadings > 0)
                    {
                        _logger.LogInformation(
                            "Background cycle completed. Processed {Count} telemetry readings.",
                            processedReadings);
                    }
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Expected when the API is shutting down.
            }
            finally
            {
                _logger.LogInformation(
                    "Smart-X background telemetry processing has stopped.");
            }
        }

        private void ProcessPendingAlerts()
        {
            // The priority queue returns the most important alert first.
            while (_alertQueue.TryDequeueAlert(out var alert)
                   && alert != null)
            {
                _logger.LogWarning(
                    "Processed {Severity} alert for device {DeviceIdentifier}. " +
                    "Value: {Value}, Message: {Message}",
                    alert.Severity,
                    alert.DeviceIdentifier,
                    alert.Value,
                    alert.Message);
            }
        }
    }
}
