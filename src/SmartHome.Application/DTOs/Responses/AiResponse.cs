using System;

namespace SmartHome.Application.DTOs.Responses;

public class AiControlResponse
{
    public string Status { get; set; } = string.Empty; // SUCCESS, AMBIGUOUS, NOT_SUPPORTED, NOT_FOUND
    public string ResponseMessage { get; set; } = string.Empty;
}
