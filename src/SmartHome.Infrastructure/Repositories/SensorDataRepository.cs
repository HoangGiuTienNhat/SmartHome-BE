using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;
using SmartHome.Infrastructure.Data;

namespace SmartHome.Infrastructure.Repositories;

public class SensorDataRepository : ISensorDataRepository
{
    private readonly SmartHomeDbContext _context;

    public SensorDataRepository(SmartHomeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SensorData>> GetDataForDeviceAsync(Guid deviceId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.SensorData.Where(sd => sd.SensorDeviceId == deviceId);

        if (startDate.HasValue)
        {
            query = query.Where(sd => sd.Time >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(sd => sd.Time <= endDate.Value);
        }

        return await query.OrderBy(sd => sd.Time).ToListAsync();
    }

    public async Task AddAsync(SensorData sensorData)
    {
        await _context.SensorData.AddAsync(sensorData);
        await _context.SaveChangesAsync();
    }
}
