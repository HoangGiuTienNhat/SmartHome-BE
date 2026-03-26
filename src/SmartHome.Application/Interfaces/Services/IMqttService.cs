namespace SmartHome.Application.Interfaces.Services;

public interface IMqttService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task PublishAsync(string feedKey, string value);
    Task SubscribeAsync(string feedKey);
}
