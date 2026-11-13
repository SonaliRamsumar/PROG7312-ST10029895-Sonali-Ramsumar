
using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class NetworkConnectionRequest
    {
        [Required]
        public string FirstDevice { get; set; } = string.Empty;

        [Required]
        public string SecondDevice { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Weight { get; set; }
    }
}
