using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface IAiLogRepository
{
    Task AddAsync(AiLog log);
    Task SaveChangesAsync();
}
