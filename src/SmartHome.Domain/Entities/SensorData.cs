namespace SmartHome.Domain.Entities;

public class SensorData
{
    public Guid Id { get; set; }
    public Guid SensorDeviceId { get; set; }
    public DateTime Time { get; set; }
    public decimal Value { get; set; }

    public Sensor Sensor { get; set; } = null!;
}
