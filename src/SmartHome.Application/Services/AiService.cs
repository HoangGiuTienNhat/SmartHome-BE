using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SmartHome.Application.DTOs.AI;
using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Interfaces.Repositories;

namespace SmartHome.Application.Services;

public class AiService : IAiService
{
    private readonly IAiProvider _aiProvider;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IAiLogRepository _aiLogRepository;
    private readonly IDeviceService _deviceService;

    public AiService(
        IAiProvider aiProvider,
        IDeviceRepository deviceRepository,
        IRoomRepository roomRepository,
        IAiLogRepository aiLogRepository,
        IDeviceService deviceService)
    {
        _aiProvider = aiProvider;
        _deviceRepository = deviceRepository;
        _roomRepository = roomRepository;
        _aiLogRepository = aiLogRepository;
        _deviceService = deviceService;
    }

    public async Task<AiControlResponse> ProcessCommandAsync(Guid userId, AiControlRequest request)
    {
        // 1. Prepare Context: Get Rooms and Output Devices for the user
        var rooms = (await _roomRepository.GetAllByUserIdAsync(userId)).ToList();
        var devices = (await _deviceRepository.GetOutputDevicesByUserIdAsync(userId)).ToList();

        // 2. Call AI Provider to analyze the command
        var analysisResult = await _aiProvider.AnalyzeCommandAsync(request.Command, rooms, devices);

        // 3. Execute actions if analysis was successful
        if (analysisResult.Status == "SUCCESS" && !string.IsNullOrEmpty(analysisResult.Action))
        {
            await ExecuteActionAsync(userId, analysisResult);
        }

        // 4. Log the interaction
        var log = new AiLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RawCommand = request.Command,
            AiResponse = JsonSerializer.Serialize(analysisResult),
            Status = analysisResult.Status,
            CreatedAt = DateTime.UtcNow
        };
        await _aiLogRepository.AddAsync(log);
        await _aiLogRepository.SaveChangesAsync();

        // 5. Map result to API response
        return new AiControlResponse
        {
            Status = analysisResult.Status,
            ResponseMessage = analysisResult.ResponseMessage
        };
    }

    private async Task ExecuteActionAsync(Guid userId, AiAnalysisResult result)
    {
        var controlRequest = new ControlDeviceRequest
        {
            Status = result.Action!
        };

        switch (result.TargetType)
        {
            case "DEVICE":
                if (result.TargetId.HasValue)
                {
                    await _deviceService.ControlDeviceAsync(userId, result.TargetId.Value, controlRequest);
                }
                break;

            case "ROOM":
                if (result.TargetId.HasValue)
                {
                    var devicesInRoom = await _deviceRepository.GetAllByRoomIdAsync(result.TargetId.Value);
                    foreach (var device in devicesInRoom)
                    {
                        // Only control output devices
                        if (device.Type == SmartHome.Domain.Enums.DeviceType.OUTPUT)
                        {
                            await _deviceService.ControlDeviceAsync(userId, device.DeviceId, controlRequest);
                        }
                    }
                }
                break;

            case "GLOBAL":
                var allOutputDevices = await _deviceRepository.GetOutputDevicesByUserIdAsync(userId);
                foreach (var device in allOutputDevices)
                {
                    await _deviceService.ControlDeviceAsync(userId, device.DeviceId, controlRequest);
                }
                break;
        }
    }
}
