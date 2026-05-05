namespace SmartHome.Application.DTOs.Requests;

public class CreateDeviceRequest
{
    public string DeviceName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Output" or "Sensor"
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
}

public class UpdateDeviceRequest
{
    public string? DeviceName { get; set; }
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
}

public class ControlDeviceRequest
{
    public string Status { get; set; } = string.Empty; // "ON", "OFF", "AUTO"
    public decimal? Value { get; set; }
}
