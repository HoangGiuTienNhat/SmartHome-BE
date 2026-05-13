using System;

namespace SmartHome.Domain.Entities;

public class AiLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string RawCommand { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty; // Store the JSON response from AI
    public string Status { get; set; } = string.Empty; // SUCCESS, AMBIGUOUS, NOT_SUPPORTED, etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
