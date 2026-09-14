namespace SmartX.Api.Models
{
    public class CombineTelemetryRequest
    {
        public TelemetryReading FirstReading { get; set; } = new();

        public TelemetryReading SecondReading { get; set; } = new();
    }
}