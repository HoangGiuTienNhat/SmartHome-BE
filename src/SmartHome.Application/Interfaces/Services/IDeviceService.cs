using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;

namespace SmartHome.Application.Interfaces.Services;

public interface IDeviceService
{
    Task<IEnumerable<DeviceResponse>> GetDevicesForRoomAsync(Guid userId, Guid roomId);
    Task<DeviceResponse> CreateDeviceAsync(Guid userId, Guid roomId, CreateDeviceRequest request);
    Task<DeviceResponse> UpdateDeviceAsync(Guid userId, Guid deviceId, UpdateDeviceRequest request);
    Task DeleteDeviceAsync(Guid userId, Guid deviceId);
    Task ControlDeviceAsync(Guid userId, Guid deviceId, ControlDeviceRequest request);
    Task<DeviceResponse?> GetDeviceByIdAsync(Guid userId, Guid deviceId);
    Task<IEnumerable<ActionLogResponse>> GetLogsAsync(Guid userId, int page, int limit);
    Task<IEnumerable<ActionLogResponse>> GetLogsByDeviceIdAsync(Guid userId, Guid deviceId, int page, int limit);
}
