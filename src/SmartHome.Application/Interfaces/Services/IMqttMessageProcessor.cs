namespace SmartHome.Application.Interfaces.Services;

public interface IMqttMessageProcessor
{
    Task ProcessMessageAsync(string feedKey, string payload);
}
