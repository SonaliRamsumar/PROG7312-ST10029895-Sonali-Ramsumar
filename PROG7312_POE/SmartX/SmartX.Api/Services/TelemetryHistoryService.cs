namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        public List<double> ConvertHistoricalBatchesToList(double[][] batches)
        {
            var readings = new List<double>();

            // Each inner array represents one batch of telemetry readings.
            foreach (var batch in batches)
            {
                readings.AddRange(batch);
            }

            return readings;
        }
    }
}