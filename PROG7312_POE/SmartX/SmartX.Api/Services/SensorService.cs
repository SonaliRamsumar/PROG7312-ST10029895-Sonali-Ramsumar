
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class SensorService
    {
        // The device identifier is the key used to find each sensor.
        private readonly Dictionary<string, SensorRegistration> _sensors =
            new(StringComparer.OrdinalIgnoreCase);

        public List<SensorRegistration> GetAll()
        {
            return _sensors.Values.ToList();
        }

        public bool DeviceExists(string deviceIdentifier)
        {
            // Dictionary lookup checks the identifier without case sensitivity.
            return !string.IsNullOrWhiteSpace(deviceIdentifier)
                && _sensors.ContainsKey(deviceIdentifier.Trim());
        }

        public SensorRegistration Add(SensorRegistration sensor)
        {
            sensor.DeviceIdentifier = sensor.DeviceIdentifier.Trim();

            if (_sensors.ContainsKey(sensor.DeviceIdentifier))
            {
                throw new InvalidOperationException(
                    "A sensor with this device identifier is already registered.");
            }

            sensor.Id = _sensors.Count + 1;

            _sensors.Add(sensor.DeviceIdentifier, sensor);

            return sensor;
        }
    }
}
