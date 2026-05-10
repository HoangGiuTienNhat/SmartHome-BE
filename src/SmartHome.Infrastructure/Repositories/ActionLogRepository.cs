using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;
using SmartHome.Infrastructure.Data;

namespace SmartHome.Infrastructure.Repositories;

public class ActionLogRepository : IActionLogRepository
{
    private readonly SmartHomeDbContext _context;

    public ActionLogRepository(SmartHomeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ActionLog>> GetLogsAsync(Guid userId, int page, int limit)
    {
        return await _context.ActionLogs
            .Where(l => l.Device.Room.RuserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(ActionLog log)
    {
        await _context.ActionLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActionLog>> GetLogsByDeviceIdAsync(Guid userId, Guid deviceId, int page = 1, int limit = 20)
    {
        // 1. Kiểm tra quyền sở hữu thiết bị trước để đảm bảo an toàn
        var ownsDevice = await _context.Devices
            .AnyAsync(d => d.DeviceId == deviceId && d.Room.RuserId == userId);
            
        if (!ownsDevice) return Enumerable.Empty<ActionLog>();

        // 2. Lấy logs cho thiết bị đó (không cần join phức tạp nếu chỉ lấy dữ liệu log)
        return await _context.ActionLogs
            .Where(log => log.LogdeviceId == deviceId)
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }
}
