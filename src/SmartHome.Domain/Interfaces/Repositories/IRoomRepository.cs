using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAllByUserIdAsync(Guid userId);
    Task<Room?> GetByIdAsync(Guid roomId);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Room room);
}
