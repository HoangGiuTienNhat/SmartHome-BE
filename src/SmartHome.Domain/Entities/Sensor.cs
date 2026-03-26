namespace SmartHome.Domain.Entities;

public class Sensor : Device
{
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }

    public ICollection<SensorData> SensorData { get; set; } = new List<SensorData>();
}
