using SmartHome.Domain.Enums;

namespace SmartHome.Domain.Entities;

public abstract class Device
{
    public Guid DeviceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FeedKey { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DeviceType Type { get; set; }
    public DateTime InstallDate { get; set; }
    public DateTime UpdateDate { get; set; }
    
    public Guid DroomId { get; set; }
    public Room Room { get; set; } = null!;
}
