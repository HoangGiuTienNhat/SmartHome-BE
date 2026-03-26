namespace SmartHome.Domain.Entities;

public class Room
{
    public Guid RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid RuserId { get; set; }
    
    public User User { get; set; } = null!;
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
