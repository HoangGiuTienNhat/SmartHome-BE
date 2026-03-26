using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;

namespace SmartHome.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<RoomResponse>> GetRoomsAsync(Guid userId)
    {
        var rooms = await _roomRepository.GetAllByUserIdAsync(userId);
        return rooms.Select(r => new RoomResponse
        {
            RoomId = r.RoomId,
            RoomName = r.Name
        });
    }

    public async Task<RoomResponse> CreateRoomAsync(Guid userId, CreateRoomRequest request)
    {
        var room = new Room
        {
            RoomId = Guid.NewGuid(),
            Name = request.RoomName,
            RuserId = userId
        };

        await _roomRepository.AddAsync(room);

        return new RoomResponse
        {
            RoomId = room.RoomId,
            RoomName = room.Name
        };
    }

    public async Task<RoomResponse> UpdateRoomAsync(Guid userId, Guid roomId, UpdateRoomRequest request)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null || room.RuserId != userId)
        {
            throw new Exception("Room not found or unauthorized.");
        }

        room.Name = request.RoomName;
        await _roomRepository.UpdateAsync(room);

        return new RoomResponse
        {
            RoomId = room.RoomId,
            RoomName = room.Name
        };
    }

    public async Task DeleteRoomAsync(Guid userId, Guid roomId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null || room.RuserId != userId)
        {
            throw new Exception("Room not found or unauthorized.");
        }

        await _roomRepository.DeleteAsync(room);
    }
}
