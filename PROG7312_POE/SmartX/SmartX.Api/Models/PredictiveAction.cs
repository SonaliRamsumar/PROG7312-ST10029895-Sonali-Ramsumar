
namespace SmartX.Api.Models
{
    // Represents an action suggested from previously recorded activity.
    public class PredictiveAction
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string DeviceIdentifier { get; set; } = string.Empty;
        public string CommandName { get; set; } = string.Empty;
        public string SuggestedValue { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;
        public string Category { get; set; } = "All";
        public string StateFilter { get; set; } = "All";

        public int EvidenceCount { get; set; }
        public int Confidence { get; set; }
        public DateTime LastObserved { get; set; }
    }
}
