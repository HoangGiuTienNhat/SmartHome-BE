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

    public async Task<IEnumerable<ActionLog>> GetLogsAsync(int page, int limit)
    {
        return await _context.ActionLogs
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
}
