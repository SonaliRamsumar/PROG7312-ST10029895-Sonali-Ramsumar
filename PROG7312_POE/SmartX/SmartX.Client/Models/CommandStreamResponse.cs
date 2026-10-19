
namespace SmartX.Client.Models
{
    public class CommandStreamResponse
    {
        // All sensors registered with the API.
        public List<SensorRegistration> Devices { get; set; } = new();

        // Current command values, grouped by device identifier.
        public Dictionary<string, Dictionary<string, string>>
            CommandStates
        { get; set; } = new();

        // Commands currently available in the undo history.
        public List<ManualCommand> CommandHistory { get; set; } = new();
    }
}
