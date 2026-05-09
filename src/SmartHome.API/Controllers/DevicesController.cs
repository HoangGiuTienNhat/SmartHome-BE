using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Domain.Interfaces.Repositories;

namespace SmartHome.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IActionLogRepository _actionLogRepository;
    private readonly ISensorDataRepository _sensorDataRepository;

    public DevicesController(
        IDeviceService deviceService,
        IActionLogRepository actionLogRepository,
        ISensorDataRepository sensorDataRepository)
    {
        _deviceService = deviceService;
        _actionLogRepository = actionLogRepository;
        _sensorDataRepository = sensorDataRepository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet("rooms/{roomId}/devices")]
    public async Task<IActionResult> GetDevicesForRoom(Guid roomId)
    {
        var devices = await _deviceService.GetDevicesForRoomAsync(GetUserId(), roomId);
        return Ok(devices);
    }

    [HttpPost("rooms/{roomId}/devices")]
    public async Task<IActionResult> CreateDevice(Guid roomId, [FromBody] CreateDeviceRequest request)
    {
        var device = await _deviceService.CreateDeviceAsync(GetUserId(), roomId, request);
        return Ok(device);
    }

    [HttpPut("devices/{deviceId}")]
    public async Task<IActionResult> UpdateDevice(Guid deviceId, [FromBody] UpdateDeviceRequest request)
    {
        var device = await _deviceService.UpdateDeviceAsync(GetUserId(), deviceId, request);
        return Ok(device);
    }

    [HttpDelete("devices/{deviceId}")]
    public async Task<IActionResult> DeleteDevice(Guid deviceId)
    {
        await _deviceService.DeleteDeviceAsync(GetUserId(), deviceId);
        return NoContent();
    }

    [HttpPost("devices/{deviceId}/control")]
    public async Task<IActionResult> ControlDevice(Guid deviceId, [FromBody] ControlDeviceRequest request)
    {
        await _deviceService.ControlDeviceAsync(GetUserId(), deviceId, request);
        return Ok(new { message = "Control command sent successfully." });
    }

    [HttpGet("devices/{deviceId}/data")]
    public async Task<IActionResult> GetSensorData(Guid deviceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = GetUserId();

        // 1. Kiểm tra thiết bị có tồn tại và thuộc quyền sở hữu của user không
        var device = await _deviceService.GetDeviceByIdAsync(userId, deviceId);
        if (device == null)
        {
            return NotFound(new { message = $"Device with ID {deviceId} not found or you don't have permission to access it." });
        }

        // 2. Kiểm tra xem có phải là Sensor không (Hướng A: Strict Sensor Only)
        if (device.Type != "SENSOR")
        {
            return BadRequest(new { 
                message = "Invalid device type.", 
                detail = $"Device '{device.DeviceName}' is a {device.Type}. Historical data API only supports SENSOR devices." 
            });
        }

        // 3. Lấy dữ liệu
        var data = await _sensorDataRepository.GetDataForDeviceAsync(userId, deviceId, startDate, endDate);
        return Ok(data);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var logs = await _deviceService.GetLogsAsync(GetUserId(), page, limit);
        return Ok(logs);
    }


    [HttpGet("devices/{deviceId}/logs")]
    public async Task<IActionResult> GetDeviceLogs(Guid deviceId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var logs = await _deviceService.GetLogsByDeviceIdAsync(GetUserId(), deviceId, page, limit);
        return Ok(logs);
    }
}
