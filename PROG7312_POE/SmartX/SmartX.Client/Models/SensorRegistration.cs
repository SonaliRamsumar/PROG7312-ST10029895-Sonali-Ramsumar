namespace SmartX.Client.Models
{
    public class SensorRegistration
    {
        public int Id { get; set; }

        public string SensorName { get; set; } = string.Empty;

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string DeploymentLocation { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}