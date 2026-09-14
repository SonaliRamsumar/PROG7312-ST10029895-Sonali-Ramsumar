using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class SensorService
    {
        private readonly List<SensorRegistration> _sensors = new();

        public List<SensorRegistration> GetAll()
        {
            return _sensors;
        }

        public SensorRegistration Add(SensorRegistration sensor)
        {
            sensor.Id = _sensors.Count + 1;
            _sensors.Add(sensor);
            return sensor;
        }
    }
}