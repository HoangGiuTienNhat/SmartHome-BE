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

    public async Task ProcessMessageAsync(string feedKey, string payload)
    {
        var device = await _deviceRepository.GetByFeedKeyAsync(feedKey);
        if (device == null || device is not Sensor sensor)
        {
            _logger.LogWarning("FeedKey {FeedKey} not mapped to any Sensor device.", feedKey);
            return;
        }

        if (!decimal.TryParse(payload, out decimal value))
        {
            _logger.LogWarning("Invalid payload {Payload} for feed {FeedKey}", payload, feedKey);
            return;
        }

        // 1. Save sensor data
        var sensorData = new SensorData
        {
            Id = Guid.NewGuid(),
            SensorDeviceId = sensor.DeviceId,
            Time = DateTime.UtcNow,
            Value = value
        };
        await _sensorDataRepository.AddAsync(sensorData);

        // 2. Automation Logic check
        await VerifyAutomationThresholds(sensor, value);
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
