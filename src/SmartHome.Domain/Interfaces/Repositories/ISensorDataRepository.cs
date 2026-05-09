using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface ISensorDataRepository
{
    Task<IEnumerable<SensorDataDto>> GetDataForDeviceAsync(Guid userId, Guid deviceId, DateTime? startDate, DateTime? endDate);
    Task AddAsync(SensorData sensorData);
}
