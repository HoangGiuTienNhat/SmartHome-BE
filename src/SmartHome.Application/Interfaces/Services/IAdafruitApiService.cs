namespace SmartHome.Application.Interfaces.Services;

public interface IAdafruitApiService
{
    Task<bool> CreateFeedAsync(string name, string key);
}
