using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class SensorRegistration
    {
        public int Id { get; set; }

        [Required]
        public string SensorName { get; set; } = string.Empty;

        [Required]
        public string DeviceIdentifier { get; set; } = string.Empty;

        [Required]
        public string DeploymentLocation { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;
    }
}