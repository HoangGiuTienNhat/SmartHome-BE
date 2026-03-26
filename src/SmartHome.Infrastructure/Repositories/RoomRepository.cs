using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;
using SmartHome.Infrastructure.Data;

namespace SmartHome.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly SmartHomeDbContext _context;

    public RoomRepository(SmartHomeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Rooms
            .Where(r => r.RuserId == userId)
            .ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(Guid roomId)
    {
        return await _context.Rooms.FindAsync(roomId);
    }

    public async Task AddAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
}
