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
            bool incomingAuto = outputDevice.Auto;

            if (cleanPayload == "AUTO")
            {
                incomingStatus = DeviceStatus.AUTO;
                incomingAuto = true;
            }
            else if (decimal.TryParse(cleanPayload, out decimal numericValue))
            {
                incomingStatus = numericValue > 0 ? DeviceStatus.ON : DeviceStatus.OFF;
                incomingValue = numericValue;
                
                // CHỈ CHUYỂN SANG MANUAL NẾU CÓ SỰ THAY ĐỔI TRẠNG THÁI THỰC SỰ
                // Nếu tin nhắn nhận về giống hệt trạng thái DB hiện tại, coi đó là tin nhắn xác nhận (Echo)
                if (outputDevice.Auto && (incomingStatus != outputDevice.OnOffState || incomingValue != outputDevice.CurrentValue))
                {
                    incomingAuto = false;
                }
            }
            else
            {
                incomingStatus = (cleanPayload == "ON" || cleanPayload == "TRUE") 
                                ? DeviceStatus.ON 
                                : DeviceStatus.OFF;
                
                // Tương tự, kiểm tra can thiệp thủ công
                if (outputDevice.Auto && incomingStatus != outputDevice.OnOffState)
                {
                    incomingAuto = false;
                }
            }

            if (outputDevice.OnOffState != incomingStatus || outputDevice.CurrentValue != incomingValue || outputDevice.Auto != incomingAuto)
            {
                bool modeChanged = outputDevice.Auto != incomingAuto;
                outputDevice.OnOffState = incomingStatus;
                outputDevice.CurrentValue = incomingValue;
                outputDevice.Auto = incomingAuto;
                outputDevice.UpdateDate = DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(outputDevice);

                var log = new ActionLog
                {
                    LogsId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    LogType = LogType.MANUAL,
                    DeviceName = outputDevice.Name,
                    Action = modeChanged && !incomingAuto ? "Override Manual" : $"Sync {incomingStatus}",
                    Detail = incomingAuto 
                        ? $"Device state confirmed/synced in AUTO mode from Adafruit."
                        : (modeChanged 
                            ? $"User intervened via external dashboard. Mode switched to MANUAL. State: {incomingStatus}"
                            : $"Device state synced to {incomingStatus} in MANUAL mode from Adafruit."),
                    LogdeviceId = outputDevice.DeviceId
                };
                await _actionLogRepository.AddAsync(log);
                
                _logger.LogInformation($"[SYNC] {outputDevice.Name} -> Status: {incomingStatus}, Auto: {incomingAuto} (From Adafruit)");
            }
        }
    }



    private async Task VerifyAutomationThresholds(Sensor sensor, decimal value)
    {
        if (!sensor.ThresholdMax.HasValue && !sensor.ThresholdMin.HasValue) return;

        // Lấy các thiết bị output được map với sensor này và đang ở chế độ AUTO
        var linkedDevices = await _deviceRepository.GetOutputDevicesBySensorIdAsync(sensor.DeviceId);
        var autoOutputs = linkedDevices.Where(d => d.Auto).ToList();

        foreach (var output in autoOutputs)
        {
            DeviceStatus? targetStatus = null;
            string payload = "0";

            // KỊCH BẢN 1: Vượt Max thì Bật, Dưới Min thì Tắt
            // KỊCH BẢN 1: Vượt Max thì Bật (50), Dưới Min thì Tắt (0)
            if (sensor.ThresholdMax.HasValue && value > sensor.ThresholdMax.Value)
            {
                // Nếu đang OFF hoặc đang ở trạng thái AUTO (chưa xác định ON/OFF)
                if (output.OnOffState == DeviceStatus.OFF || output.OnOffState == DeviceStatus.AUTO)
                {
                    targetStatus = DeviceStatus.ON;
                    payload = "50";
                }
            }
            else if (sensor.ThresholdMin.HasValue && value < sensor.ThresholdMin.Value)
            {
                // Nếu đang ON hoặc đang ở trạng thái AUTO
                if (output.OnOffState == DeviceStatus.ON || output.OnOffState == DeviceStatus.AUTO)
                {
                    targetStatus = DeviceStatus.OFF;
                    payload = "0";
                }
            }

            if (targetStatus.HasValue)
            {
                output.OnOffState = targetStatus.Value;
                output.CurrentValue = (targetStatus.Value == DeviceStatus.ON) ? 50 : 0;
                output.UpdateDate = DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(output);

                await _mqttService.PublishAsync(output.FeedKey, payload);

                var log = new ActionLog
                {
                    LogsId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    LogType = LogType.AUTO,
                    DeviceName = output.Name,
                    Action = $"Auto Turn {targetStatus}",
                    Detail = $"Sensor {sensor.Name} value {value} triggered automation. Thresholds: [Min: {sensor.ThresholdMin}, Max: {sensor.ThresholdMax}]",
                    LogdeviceId = output.DeviceId
                };
                await _actionLogRepository.AddAsync(log);
                
                _logger.LogInformation($"[AUTO] Sensor {sensor.Name} ({value}) triggered {output.Name} to {targetStatus} (CurrentValue: {output.CurrentValue})");
            }
        }
    }
}
