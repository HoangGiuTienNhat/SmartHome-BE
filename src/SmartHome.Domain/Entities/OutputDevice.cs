using SmartHome.Domain.Enums;

namespace SmartHome.Domain.Entities;

public class OutputDevice : Device
{
    public bool Auto { get; set; }
    public DeviceStatus OnOffState { get; set; }
}
