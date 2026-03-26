using SmartHome.Domain.Entities;

namespace SmartHome.Domain.Interfaces.Repositories;

public interface IActionLogRepository
{
    Task<IEnumerable<ActionLog>> GetLogsAsync(int page, int limit);
    Task AddAsync(ActionLog log);

    // THÊM HÀM NÀY: Lấy log theo DeviceId
    Task<IEnumerable<ActionLog>> GetLogsByDeviceIdAsync(Guid deviceId, int page = 1, int limit = 20);
}
