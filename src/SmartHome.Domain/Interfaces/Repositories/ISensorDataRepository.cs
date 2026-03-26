using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface ISensorDataRepository
{
    Task<IEnumerable<SensorData>> GetDataForDeviceAsync(Guid deviceId, DateTime? startDate, DateTime? endDate);
    Task AddAsync(SensorData sensorData);
}
