namespace SmartX.Api.Models
{
    public class SensorRegistration
    {
        public int Id { get; set; }

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string DeploymentLocation { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}