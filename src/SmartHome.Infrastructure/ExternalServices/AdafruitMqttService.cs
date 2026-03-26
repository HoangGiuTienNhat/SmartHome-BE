using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using SmartHome.Application.Interfaces.Services;

namespace SmartHome.Infrastructure.ExternalServices;

public class AdafruitMqttService : IMqttService
{
    private IMqttClient? _mqttClient;
    private MqttClientOptions? _mqttOptions;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdafruitMqttService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _username;
    private readonly string _key;

    public AdafruitMqttService(IConfiguration configuration, ILogger<AdafruitMqttService> logger, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
        
        _username = _configuration["Adafruit:Username"] ?? throw new ArgumentNullException("Adafruit:Username config missing");
        _key = _configuration["Adafruit:Key"] ?? throw new ArgumentNullException("Adafruit:Key config missing");
    }

    public async Task ConnectAsync()
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("io.adafruit.com", 1883)
            .WithCredentials(_username, _key)
            .WithClientId(Guid.NewGuid().ToString())
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            _logger.LogInformation("Received MQTT msg on {Topic}: {Payload}", topic, payload);

            // Extract feedKey from topic (io.adafruit.com format: username/f/feedKey)
            var parts = topic.Split('/');
            if (parts.Length >= 3 && parts[1] == "f")
            {
                var feedKey = parts[2];
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IMqttMessageProcessor>();
                await processor.ProcessMessageAsync(feedKey, payload);
            }
        };

        _mqttClient.DisconnectedAsync += async e =>
        {
            _logger.LogWarning("MQTT disconnected. Reconnecting in 5s...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT reconnect failed");
            }
        };

        try
        {
            await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
            _logger.LogInformation("Connected to Adafruit IO via MQTT.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Adafruit IO.");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_mqttClient != null)
        {
            await _mqttClient.DisconnectAsync();
            _mqttClient.Dispose();
        }
    }

    public async Task PublishAsync(string feedKey, string value)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
        {
            _logger.LogWarning("Cannot publish, MQTT client not connected.");
            return;
        }

        var topic = $"{_username}/f/{feedKey}";
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(value)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await _mqttClient.PublishAsync(message, CancellationToken.None);
        _logger.LogInformation("Published to {Topic}: {Value}", topic, value);
    }

    public async Task SubscribeAsync(string feedKey)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
        {
            _logger.LogWarning("Cannot subscribe, MQTT client not connected.");
            return;
        }

        var topic = $"{_username}/f/{feedKey}";
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topic)
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);
    }
}
