using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetAllByRoomIdAsync(Guid roomId);
    Task<Device?> GetByIdAsync(Guid deviceId);
    Task<Device?> GetByFeedKeyAsync(string feedKey);
    Task<bool> IsFeedKeyExistsAsync(string feedKey);
    Task<bool> IsNameExistsInRoomAsync(Guid roomId, string name);
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task DeleteAsync(Device device);
}
