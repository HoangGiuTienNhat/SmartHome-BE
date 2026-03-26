using SmartHome.Application.Interfaces.Services;

namespace SmartHome.API.BackgroundServices;

public class MqttHostedService : IHostedService
{
    private readonly IMqttService _mqttService;
    private readonly ILogger<MqttHostedService> _logger;

    public MqttHostedService(IMqttService mqttService, ILogger<MqttHostedService> logger)
    {
        _mqttService = mqttService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT Background Service...");
        await _mqttService.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT Background Service...");
        await _mqttService.DisconnectAsync();
    }
}
