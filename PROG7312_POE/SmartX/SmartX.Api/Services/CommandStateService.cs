
namespace SmartX.Api.Services
{
    public class CommandStateService
    {
        // Store command values separately for each device.
        // Device and command names are compared without case sensitivity.
        private readonly Dictionary<string, Dictionary<string, string>> _deviceStates =
            new(StringComparer.OrdinalIgnoreCase);

        // Get the current value of a command for one device.
        public string? GetValue(string deviceIdentifier, string commandName)
        {
            if (_deviceStates.TryGetValue(deviceIdentifier, out var commands)
                && commands.TryGetValue(commandName, out var value))
            {
                return value;
            }

            return null;
        }

        // Store or update a command value for a device.
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

        // Return the current command states for all devices.
        public Dictionary<string, Dictionary<string, string>> GetAllStates()
        {
            return _deviceStates.ToDictionary(
                device => device.Key,
                device => new Dictionary<string, string>(
                    device.Value,
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
