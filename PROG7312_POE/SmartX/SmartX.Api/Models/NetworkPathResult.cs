
namespace SmartX.Api.Models
{
    public class NetworkPathResult
    {
        public string StartDevice { get; set; } = string.Empty;

        public string DestinationDevice { get; set; } = string.Empty;

        public List<string> Path { get; set; } = new();

        public double? TotalWeight { get; set; }

        public bool Found { get; set; }
    }
}
