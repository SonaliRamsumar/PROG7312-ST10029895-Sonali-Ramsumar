
using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.Models
{
    public class ManualCommandRequest
    {
        [Required]
        public string DeviceIdentifier { get; set; } = string.Empty;

        [Required]
        public string CommandName { get; set; } = string.Empty;

        [Required]
        public string NewValue { get; set; } = string.Empty;
    }
}
