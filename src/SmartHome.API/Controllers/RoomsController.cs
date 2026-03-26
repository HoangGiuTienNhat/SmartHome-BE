using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.Interfaces.Services;

namespace SmartHome.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _roomService.GetRoomsAsync(GetUserId());
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var room = await _roomService.CreateRoomAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetRooms), new { id = room.RoomId }, room);
    }

    [HttpPut("{roomId}")]
    public async Task<IActionResult> UpdateRoom(Guid roomId, [FromBody] UpdateRoomRequest request)
    {
        var room = await _roomService.UpdateRoomAsync(GetUserId(), roomId, request);
        return Ok(room);
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        await _roomService.DeleteRoomAsync(GetUserId(), roomId);
        return NoContent();
    }
}
