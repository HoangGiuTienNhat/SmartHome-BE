using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Application.Utils;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Enums;
using SmartHome.Domain.Interfaces.Repositories;

namespace SmartHome.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IActionLogRepository _actionLogRepository;
    private readonly IMqttService _mqttService;
    private readonly IAdafruitApiService _adafruitApiService;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IRoomRepository roomRepository,
        IActionLogRepository actionLogRepository,
        IMqttService mqttService,
        IAdafruitApiService adafruitApiService)
    {
        _deviceRepository = deviceRepository;
        _roomRepository = roomRepository;
        _actionLogRepository = actionLogRepository;
        _mqttService = mqttService;
        _adafruitApiService = adafruitApiService;
    }

    public async Task<IEnumerable<DeviceResponse>> GetDevicesForRoomAsync(Guid userId, Guid roomId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null || room.RuserId != userId)
        {
            throw new Exception("Room not found or unauthorized.");
        }

        var devices = await _deviceRepository.GetAllByRoomIdAsync(roomId);
        return devices.Select(MapToResponse);
    }

    public async Task<DeviceResponse> CreateDeviceAsync(Guid userId, Guid roomId, CreateDeviceRequest request)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null || room.RuserId != userId)
        {
            throw new Exception("Room not found or unauthorized.");
        }

        if (await _deviceRepository.IsNameExistsInRoomAsync(roomId, request.DeviceName))
        {
            throw new Exception($"Device name '{request.DeviceName}' already exists in this room.");
        }

        string rawSlug = StringHelper.GenerateSlug(room.Name + " " + request.DeviceName);
        string finalSlug = rawSlug;
        int counter = 2;

        while (await _deviceRepository.IsFeedKeyExistsAsync(finalSlug))
        {
            finalSlug = $"{rawSlug}-{counter}";
            counter++;
        }

        Device device;
        if (request.Type.Equals("Output", StringComparison.OrdinalIgnoreCase))
        {
            device = new OutputDevice
            {
                DeviceId = Guid.NewGuid(),
                Name = request.DeviceName,
                FeedKey = finalSlug,
                State = "CONNECTED",
                Type = DeviceType.OUTPUT,
                InstallDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                DroomId = roomId,
                Auto = false,
                OnOffState = DeviceStatus.OFF
            };
        }
        else if (request.Type.Equals("Sensor", StringComparison.OrdinalIgnoreCase))
        {
            device = new Sensor
            {
                DeviceId = Guid.NewGuid(),
                Name = request.DeviceName,
                FeedKey = finalSlug,
                State = "CONNECTED",
                Type = DeviceType.SENSOR,
                InstallDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                DroomId = roomId,
                ThresholdMin = request.ThresholdMin,
                ThresholdMax = request.ThresholdMax
            };
        }
        else
        {
            throw new Exception("Invalid device type.");
        }

        // 1. Tự động tạo Feed trên Adafruit IO trước khi lưu vào DB
        var success = await _adafruitApiService.CreateFeedAsync(device.Name, device.FeedKey);
        if (!success)
        {
            throw new Exception("Failed to create feed on Adafruit IO. Device creation aborted.");
        }

        await _deviceRepository.AddAsync(device);
        
        // Auto subscribe for sensor data if it's a sensor
        // if (device.Type == DeviceType.SENSOR)
        // {
        //     await _mqttService.SubscribeAsync(device.FeedKey);
        // }


        // Xóa lệnh if đi, giữ lại dòng này cho mọi thiết bị:
        await _mqttService.SubscribeAsync(device.FeedKey);
        
        return MapToResponse(device);
    }

    public async Task<DeviceResponse> UpdateDeviceAsync(Guid userId, Guid deviceId, UpdateDeviceRequest request)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new Exception("Device not found.");

        var room = await _roomRepository.GetByIdAsync(device.DroomId);
        if (room == null || room.RuserId != userId)
            throw new Exception("Unauthorized.");

        if (!string.IsNullOrEmpty(request.DeviceName))
        {
            device.Name = request.DeviceName;
        }

        if (device is Sensor sensor)
        {
            if (request.ThresholdMin.HasValue) sensor.ThresholdMin = request.ThresholdMin.Value;
            if (request.ThresholdMax.HasValue) sensor.ThresholdMax = request.ThresholdMax.Value;
        }

        device.UpdateDate = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);

        return MapToResponse(device);
    }

    public async Task DeleteDeviceAsync(Guid userId, Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new Exception("Device not found.");

        var room = await _roomRepository.GetByIdAsync(device.DroomId);
        if (room == null || room.RuserId != userId)
            throw new Exception("Unauthorized.");

        await _deviceRepository.DeleteAsync(device);
    }

    public async Task ControlDeviceAsync(Guid userId, Guid deviceId, ControlDeviceRequest request)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null || device is not OutputDevice outputDevice)
            throw new Exception("Device not found or not an output device.");

        var room = await _roomRepository.GetByIdAsync(device.DroomId);
        if (room == null || room.RuserId != userId)
            throw new Exception("Unauthorized.");

        if (!Enum.TryParse<DeviceStatus>(request.Status.ToUpper(), out var status))
        {
            throw new Exception("Invalid status. Allowed: ON, OFF, AUTO");
        }

        outputDevice.OnOffState = status;
        if (request.Value.HasValue)
        {
            outputDevice.CurrentValue = request.Value.Value;
        }
        outputDevice.UpdateDate = DateTime.UtcNow;
        
        await _deviceRepository.UpdateAsync(outputDevice);

        // Map status/value to strings expected by Adafruit
        string payloadValue;
        if (status == DeviceStatus.OFF)
        {
            payloadValue = "0";
        }
        else if (status == DeviceStatus.ON && request.Value.HasValue)
        {
            payloadValue = request.Value.Value.ToString();
        }
        else
        {
            payloadValue = status switch
            {
                DeviceStatus.ON => "1",
                DeviceStatus.AUTO => "AUTO",
                _ => "0"
            };
        }
        await _mqttService.PublishAsync(outputDevice.FeedKey, payloadValue);

        var log = new ActionLog
        {
            LogsId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            LogType = LogType.MANUAL,
            DeviceName = outputDevice.Name,
            Action = $"Turn {status}",
            Detail = request.Value.HasValue 
                ? $"User set {outputDevice.Name} to {status} with value {request.Value.Value} via Interface."
                : $"User turned {status} device '{outputDevice.Name}' via Interface.",
            LogdeviceId = outputDevice.DeviceId
        };
        await _actionLogRepository.AddAsync(log);
    }

    private DeviceResponse MapToResponse(Device device)
    {
        var response = new DeviceResponse
        {
            DeviceId = device.DeviceId,
            DeviceName = device.Name,
            FeedKey = device.FeedKey,
            Type = device.Type.ToString(),
            State = device.State
        };

        if (device is OutputDevice outputDevice)
        {
            response.Auto = outputDevice.Auto;
            response.OnOffState = outputDevice.OnOffState.ToString();
            response.CurrentValue = outputDevice.CurrentValue;
        }
        else if (device is Sensor sensor)
        {
            response.ThresholdMin = sensor.ThresholdMin;
            response.ThresholdMax = sensor.ThresholdMax;
        }

        return response;
    }
}
