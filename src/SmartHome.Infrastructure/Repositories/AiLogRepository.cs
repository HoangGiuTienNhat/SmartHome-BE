using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;
using SmartHome.Infrastructure.Data;

namespace SmartHome.Infrastructure.Repositories;

public class AiLogRepository : IAiLogRepository
{
    private readonly SmartHomeDbContext _context;

    public AiLogRepository(SmartHomeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AiLog log)
    {
        await _context.AiLogs.AddAsync(log);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
