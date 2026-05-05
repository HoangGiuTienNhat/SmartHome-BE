using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartHome.Application.Interfaces.Services;

namespace SmartHome.Infrastructure.ExternalServices;

public class AdafruitApiService : IAdafruitApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _key;
    private readonly ILogger<AdafruitApiService> _logger;

    public AdafruitApiService(HttpClient httpClient, IConfiguration configuration, ILogger<AdafruitApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _username = configuration["Adafruit:Username"] ?? throw new ArgumentNullException("Adafruit:Username missing");
        _key = configuration["Adafruit:Key"] ?? throw new ArgumentNullException("Adafruit:Key missing");
    }

    public async Task<bool> CreateFeedAsync(string name, string key)
    {
        var url = $"https://io.adafruit.com/api/v2/{_username}/feeds";
        _logger.LogInformation("Attempting to create feed on Adafruit. URL: {Url}, Username: {Username}", url, _username);
        
        var requestBody = new
        {
            feed = new
            {
                name = name,
                key = key
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("X-AIO-Key", _key);

        try
        {
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created feed on Adafruit. Response: {Content}", content);
                return true;
            }

            _logger.LogError("Failed to create feed on Adafruit. Status: {Status}, Error: {Error}", response.StatusCode, content);
            
            if (content.Contains("already in use", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Feed key '{Key}' already exists on Adafruit account '{Username}'.", key, _username);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while calling Adafruit API.");
            return false;
        }
    }
}
