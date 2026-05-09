namespace SmartHome.Domain.Entities;

public class SensorDataDto
{
    public Guid Id { get; set; }
    public Guid SensorDeviceId { get; set; }
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
}
