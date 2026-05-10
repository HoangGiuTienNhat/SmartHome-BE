using Microsoft.EntityFrameworkCore;
using SmartHome.Application.DTOs.Responses;
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

    public async Task<IEnumerable<SensorDataDto>> GetDataForDeviceAsync(Guid userId, Guid deviceId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.SensorData
            .Include(sd => sd.Sensor)
                .ThenInclude(s => s.Room)
            .Where(sd => sd.SensorDeviceId == deviceId && sd.Sensor.Room.RuserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(sd => sd.Time >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(sd => sd.Time <= endDate.Value);
        }

        return await query
            .OrderBy(sd => sd.Time)
            .Select(sd => new SensorDataDto
            {
                Id = sd.Id,
                SensorDeviceId = sd.SensorDeviceId,
                Time = sd.Time,
                Value = sd.Value
            })
            .ToListAsync();
    }

    public async Task AddAsync(SensorData sensorData)
    {
        await _context.SensorData.AddAsync(sensorData);
        await _context.SaveChangesAsync();
    }
}
