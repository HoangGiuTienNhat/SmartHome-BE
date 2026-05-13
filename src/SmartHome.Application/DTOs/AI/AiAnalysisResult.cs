using System;

namespace SmartHome.Application.DTOs.AI;

public class AiAnalysisResult
{
    public string Status { get; set; } = string.Empty;
    public string? TargetType { get; set; } // DEVICE, ROOM, GLOBAL
    public Guid? TargetId { get; set; }
    public string? Action { get; set; } // ON, OFF, AUTO
    public string ResponseMessage { get; set; } = string.Empty;
}
