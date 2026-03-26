using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Entities;
using SmartHome.Domain.Enums;

namespace SmartHome.Infrastructure.Data;

public class SmartHomeDbContext : DbContext
{
    public SmartHomeDbContext(DbContextOptions<SmartHomeDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<OutputDevice> OutputDevices { get; set; } = null!;
    public DbSet<Sensor> Sensors { get; set; } = null!;
    public DbSet<SensorData> SensorData { get; set; } = null!;
    public DbSet<ActionLog> ActionLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // 2. Rooms
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.RuserId).HasColumnName("ruser_id");

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Rooms)
                  .HasForeignKey(r => r.RuserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. Devices (Base - TPT)
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.FeedKey).HasColumnName("feed_key");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.InstallDate).HasColumnName("install_date");
            entity.Property(e => e.UpdateDate).HasColumnName("update_date");
            entity.Property(e => e.DroomId).HasColumnName("droom_id");

            entity.HasIndex(e => e.FeedKey).IsUnique();

            entity.HasOne(d => d.Room)
                  .WithMany(r => r.Devices)
                  .HasForeignKey(d => d.DroomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. OutputDevice (TPT)
        modelBuilder.Entity<OutputDevice>(entity =>
        {
            entity.ToTable("output_devices");
            entity.Property(e => e.Auto).HasColumnName("auto");
            entity.Property(e => e.OnOffState).HasColumnName("onoff_state").HasConversion<string>();
        });

        // 5. Sensor (TPT)
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensors");
            entity.Property(e => e.ThresholdMin).HasColumnName("threshold_min");
            entity.Property(e => e.ThresholdMax).HasColumnName("threshold_max");
        });

        // 6. ActionLog
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.ToTable("action_logs");
            entity.HasKey(e => e.LogsId);
            entity.Property(e => e.LogsId).HasColumnName("logs_id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.LogType).HasColumnName("log_type").HasConversion<string>();
            entity.Property(e => e.DeviceName).HasColumnName("device_name");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.Detail).HasColumnName("detail");
            entity.Property(e => e.LogdeviceId).HasColumnName("logdevice_id");

            entity.HasOne(al => al.Device)
                  .WithMany()
                  .HasForeignKey(al => al.LogdeviceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 7. SensorData
        modelBuilder.Entity<SensorData>(entity =>
        {
            entity.ToTable("sensor_data");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SensorDeviceId).HasColumnName("sensor_device_id");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(sd => sd.Sensor)
                  .WithMany(s => s.SensorData)
                  .HasForeignKey(sd => sd.SensorDeviceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        






        // ===== SEED DATA =====
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var roomId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var deviceId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = userId,
                Email = "admin@gmail.com",
                Password = "123456",
                FullName = "Admin"
            }
        );

        modelBuilder.Entity<Room>().HasData(
            new Room
            {
                RoomId = roomId,
                Name = "Living Room",
                RuserId = userId
            }
        );

        // ✅ Seed OutputDevice (bao gồm base + derived)
        modelBuilder.Entity<OutputDevice>().HasData(
            new
            {
                DeviceId = deviceId,
                Name = "Main Light",
                FeedKey = "light-1",
                State = "OFF",
                Type = DeviceType.OUTPUT, // ⚠️ sửa theo enum của bạn
                // InstallDate = new DateTime(2024, 1, 1),
                // UpdateDate = new DateTime(2024, 1, 1),
                InstallDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdateDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DroomId = roomId,
                Auto = false,
                OnOffState = DeviceStatus.OFF // ⚠️ sửa theo enum của bạn
            }
        );


    }








    
}
