using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;

namespace SmartHome.Application.Interfaces.Services;

public interface IRoomService
{
    Task<IEnumerable<RoomResponse>> GetRoomsAsync(Guid userId);
    Task<RoomResponse> CreateRoomAsync(Guid userId, CreateRoomRequest request);
    Task<RoomResponse> UpdateRoomAsync(Guid userId, Guid roomId, UpdateRoomRequest request);
    Task DeleteRoomAsync(Guid userId, Guid roomId);
}
