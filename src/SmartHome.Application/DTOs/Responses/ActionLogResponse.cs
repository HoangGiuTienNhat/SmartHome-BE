using SmartHome.Domain.Enums;

namespace SmartHome.Application.DTOs.Responses;

public class ActionLogResponse
{
    public Guid LogsId { get; set; }
    public DateTime Timestamp { get; set; }
    public LogType LogType { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public Guid? LogdeviceId { get; set; }
}
