
namespace SmartX.Client.Models
{
    public class SearchActivityRequest
    {
        public string Query { get; set; } = string.Empty;

        public string Category { get; set; } = "All";

        public string StateFilter { get; set; } = "All";
    }
}
