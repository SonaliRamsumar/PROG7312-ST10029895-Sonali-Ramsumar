
namespace SmartX.Api.Services
{
    public class CommandStateService
    {
        // Stores command values separately for each device.
        private readonly Dictionary<string, Dictionary<string, string>> _deviceStates =
            new(StringComparer.OrdinalIgnoreCase);

        public string? GetValue(string deviceIdentifier, string commandName)
        {
            if (_deviceStates.TryGetValue(deviceIdentifier, out var commands)
                && commands.TryGetValue(commandName, out var value))
            {
                return value;
            }

            // Null means this command has not been set yet.
            return null;
        }

        public void SetValue(
            string deviceIdentifier,
            string commandName,
            string value)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier)
                || string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException(
                    "A device identifier and command name are required.");
            }

            if (!_deviceStates.TryGetValue(deviceIdentifier, out var commands))
            {
                commands = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                _deviceStates[deviceIdentifier] = commands;
            }

            commands[commandName] = value;
        }
    }
}
