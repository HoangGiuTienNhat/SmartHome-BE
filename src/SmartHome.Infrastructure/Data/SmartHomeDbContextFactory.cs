using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartHome.Infrastructure.Data;

public class SmartHomeDbContextFactory : IDesignTimeDbContextFactory<SmartHomeDbContext>
{
    public SmartHomeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartHomeDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=smart_home_db;Username=postgres;Password=pass123"
        );

        return new SmartHomeDbContext(optionsBuilder.Options);
    }
}