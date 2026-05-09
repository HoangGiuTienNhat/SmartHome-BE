using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;
using SmartHome.Infrastructure.Data;

namespace SmartHome.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly SmartHomeDbContext _context;

    public DeviceRepository(SmartHomeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Device>> GetAllByRoomIdAsync(Guid roomId)
    {
        return await _context.Devices
            .Where(d => d.DroomId == roomId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OutputDevice>> GetOutputDevicesBySensorIdAsync(Guid sensorId)
    {
        return await _context.OutputDevices
            .Where(d => d.ConnectedSensorId == sensorId)
            .ToListAsync();
    }

    public async Task<Device?> GetByIdAsync(Guid deviceId)
    {
        return await _context.Devices.FindAsync(deviceId);
    }
    
    public async Task<Device?> GetByFeedKeyAsync(string feedKey)
    {
        return await _context.Devices.FirstOrDefaultAsync(d => d.FeedKey == feedKey);
    }

    public async Task<bool> IsFeedKeyExistsAsync(string feedKey)
    {
        return await _context.Devices.AnyAsync(d => d.FeedKey == feedKey);
    }

    public async Task<bool> IsNameExistsInRoomAsync(Guid roomId, string name)
    {
        return await _context.Devices.AnyAsync(d => d.DroomId == roomId && d.Name == name);
    }

    public async Task AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Device device)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Device device)
    {
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
    }
}
