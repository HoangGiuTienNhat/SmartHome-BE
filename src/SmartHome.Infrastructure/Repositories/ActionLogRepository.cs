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
            .Include(l => l.Device)
            .ThenInclude(d => d.Room)
            .Where(l => l.Device != null && l.Device.Room != null && l.Device.Room.RuserId == userId)
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
        return await _context.ActionLogs
            .Include(l => l.Device)
            .ThenInclude(d => d.Room)
            .Where(log => log.LogdeviceId == deviceId && log.Device != null && log.Device.Room != null && log.Device.Room.RuserId == userId) // Lọc đúng ID của thiết bị và User
            .OrderByDescending(log => log.Timestamp)   // Sắp xếp mới nhất lên đầu
            .Skip((page - 1) * limit)                  // Phân trang
            .Take(limit)
            .ToListAsync();
    }
}
