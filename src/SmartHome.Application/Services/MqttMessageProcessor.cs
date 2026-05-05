using Microsoft.Extensions.Logging;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Enums;
using SmartHome.Domain.Interfaces.Repositories;

namespace SmartHome.Application.Services;

public class MqttMessageProcessor : IMqttMessageProcessor
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ISensorDataRepository _sensorDataRepository;
    private readonly IActionLogRepository _actionLogRepository;
    private readonly IMqttService _mqttService;
    private readonly ILogger<MqttMessageProcessor> _logger;

    public MqttMessageProcessor(
        IDeviceRepository deviceRepository,
        ISensorDataRepository sensorDataRepository,
        IActionLogRepository actionLogRepository,
        IMqttService mqttService,
        ILogger<MqttMessageProcessor> logger)
    {
        _deviceRepository = deviceRepository;
        _sensorDataRepository = sensorDataRepository;
        _actionLogRepository = actionLogRepository;
        _mqttService = mqttService;
        _logger = logger;
    }

    // public async Task ProcessMessageAsync(string feedKey, string payload)
    // {
    //     var device = await _deviceRepository.GetByFeedKeyAsync(feedKey);
    //     if (device == null || device is not Sensor sensor)
    //     {
    //         _logger.LogWarning("FeedKey {FeedKey} not mapped to any Sensor device.", feedKey);
    //         return;
    //     }

    //     if (!decimal.TryParse(payload, out decimal value))
    //     {
    //         _logger.LogWarning("Invalid payload {Payload} for feed {FeedKey}", payload, feedKey);
    //         return;
    //     }

    //     // 1. Save sensor data
    //     var sensorData = new SensorData
    //     {
    //         Id = Guid.NewGuid(),
    //         SensorDeviceId = sensor.DeviceId,
    //         Time = DateTime.UtcNow,
    //         Value = value
    //     };
    //     await _sensorDataRepository.AddAsync(sensorData);

    //     // 2. Automation Logic check
    //     await VerifyAutomationThresholds(sensor, value);
    // }




    public async Task ProcessMessageAsync(string feedKey, string payload)
    {
        var device = await _deviceRepository.GetByFeedKeyAsync(feedKey);
        if (device == null)
        {
            _logger.LogWarning("FeedKey {FeedKey} not found in database.", feedKey);
            return;
        }

        // ==========================================
        // TRƯỜNG HỢP 1: DỮ LIỆU TỪ CẢM BIẾN (SENSOR)
        // ==========================================
        if (device is Sensor sensor)
        {
            if (!decimal.TryParse(payload, out decimal value)) return;

            // 1. Lưu data cảm biến
            var sensorData = new SensorData
            {
                Id = Guid.NewGuid(),
                SensorDeviceId = sensor.DeviceId,
                Time = DateTime.UtcNow,
                Value = value
            };
            await _sensorDataRepository.AddAsync(sensorData);

            // 2. Chạy logic tự động hóa
            await VerifyAutomationThresholds(sensor, value);
        }
        // ==========================================
        // TRƯỜNG HỢP 2: PHẢN HỒI TỪ THIẾT BỊ ĐẦU RA (OUTPUT)
        // ==========================================
        else if (device is OutputDevice outputDevice)
        {
            string cleanPayload = payload.Trim().ToUpper();
            DeviceStatus incomingStatus;
            decimal? incomingValue = outputDevice.CurrentValue;

            if (decimal.TryParse(cleanPayload, out decimal numericValue))
            {
                incomingStatus = numericValue > 0 ? DeviceStatus.ON : DeviceStatus.OFF;
                incomingValue = numericValue;
            }
            else
            {
                incomingStatus = (cleanPayload == "ON" || cleanPayload == "TRUE" || cleanPayload == "AUTO") 
                                ? DeviceStatus.ON 
                                : DeviceStatus.OFF;
                if (cleanPayload == "AUTO") incomingStatus = DeviceStatus.AUTO;
            }

            if (outputDevice.OnOffState != incomingStatus || outputDevice.CurrentValue != incomingValue)
            {
                outputDevice.OnOffState = incomingStatus;
                outputDevice.CurrentValue = incomingValue;
                outputDevice.UpdateDate = DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(outputDevice);

                var log = new ActionLog
                {
                    LogsId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    LogType = LogType.MANUAL,
                    DeviceName = outputDevice.Name,
                    Action = $"Sync {incomingStatus}",
                    Detail = incomingValue.HasValue 
                        ? $"Device state synced to {incomingStatus} with value {incomingValue} from Adafruit."
                        : $"Device state synced to {incomingStatus} from Adafruit.",
                    LogdeviceId = outputDevice.DeviceId
                };
                await _actionLogRepository.AddAsync(log);
                
                _logger.LogInformation($"[SYNC] Đã đồng bộ {outputDevice.Name} thành {incomingStatus} (Value: {incomingValue}) từ Adafruit.");
            }
        }
    }



    private async Task VerifyAutomationThresholds(Sensor sensor, decimal value)
    {
        if (!sensor.ThresholdMax.HasValue && !sensor.ThresholdMin.HasValue) return;

        bool triggered = false;
        string actionTarget = "OFF";

        // Simple mock automation rule based on thresholds
        if (sensor.ThresholdMax.HasValue && value > sensor.ThresholdMax.Value)
        {
            triggered = true;
            actionTarget = "ON"; // Example: Turn ON Cooling
        }
        else if (sensor.ThresholdMin.HasValue && value < sensor.ThresholdMin.Value)
        {
            triggered = true;
            actionTarget = "ON"; // Example: Turn ON Heater
        }
        else
        {
            // Back to normal bounds -> Auto Turn OFF
            triggered = true;
            actionTarget = "OFF";
        }

        if (triggered)
        {
            var roomDevices = await _deviceRepository.GetAllByRoomIdAsync(sensor.DroomId);
            var autoOutputs = roomDevices.OfType<OutputDevice>().Where(d => d.Auto).ToList();

            foreach (var output in autoOutputs)
            {
                DeviceStatus targetStatus = actionTarget == "ON" ? DeviceStatus.ON : DeviceStatus.OFF;
                
                if (output.OnOffState != targetStatus)
                {
                    output.OnOffState = targetStatus;
                    output.UpdateDate = DateTime.UtcNow;
                    await _deviceRepository.UpdateAsync(output);

                    await _mqttService.PublishAsync(output.FeedKey, actionTarget == "ON" ? "1" : "0");

                    var log = new ActionLog
                    {
                        LogsId = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        LogType = LogType.AUTO,
                        DeviceName = output.Name,
                        Action = $"Turn {targetStatus}",
                        Detail = $"Sensor {sensor.Name} value {value} triggered automation. Thresholds: [{sensor.ThresholdMin}, {sensor.ThresholdMax}]",
                        LogdeviceId = output.DeviceId
                    };
                    await _actionLogRepository.AddAsync(log);
                }
            }
        }
    }
}
