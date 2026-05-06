namespace SmartHome.Application.DTOs.Responses;

public class DeviceResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string FeedKey { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    
    // Output specific
    public bool? Auto { get; set; }
    public string? OnOffState { get; set; }
    public decimal? CurrentValue { get; set; }
    public Guid? ConnectedSensorId { get; set; }

    // Sensor specific
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
}
