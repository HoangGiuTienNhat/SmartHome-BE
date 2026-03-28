// using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Infrastructure.Data;
// ... các using khác

namespace SmartHome.API.BackgroundServices;

public class MqttHostedService : BackgroundService
{
    private readonly IMqttService _mqttService;
    private readonly ILogger<MqttHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MqttHostedService(
        IMqttService mqttService, 
        ILogger<MqttHostedService> logger, 
        IServiceScopeFactory scopeFactory)
    {
        _mqttService = mqttService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MQTT Background Service...");

        // 1. Gọi hàm ConnectAsync từ AdafruitMqttService của bạn
        await _mqttService.ConnectAsync();

        // 2. Chờ một chút để đảm bảo kết nối ổn định (tuỳ chọn)
        await Task.Delay(2000, stoppingToken);

        // 3. Mở một Scope để query Database lấy toàn bộ thiết bị
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();
        
        try
        {
            var devices = dbContext.Devices.ToList();
            if (devices.Any())
            {
                foreach (var device in devices)
                {
                    // Lặp qua từng thiết bị và gọi hàm SubscribeAsync từ service của bạn
                    await _mqttService.SubscribeAsync(device.FeedKey);
                }
                _logger.LogInformation($"Đã hoàn tất đăng ký Subscribe cho {devices.Count} thiết bị từ DB.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi query thiết bị để Subscribe MQTT.");
        }

        // 4. Giữ cho service chạy ngầm
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping MQTT Background Service...");
        await _mqttService.DisconnectAsync();
        await base.StopAsync(stoppingToken);
    }
}