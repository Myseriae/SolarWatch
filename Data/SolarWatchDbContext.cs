using Microsoft.EntityFrameworkCore;
using SolarWatch.Models;

namespace SolarWatch.Data;

public class SolarWatchDbContext : DbContext
{
    public SolarWatchDbContext(DbContextOptions<SolarWatchDbContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();
    public DbSet<SunriseSunset> SunriseSunsets => Set<SunriseSunset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired();
            entity.HasMany(c => c.SunriseSunsets)
                  .WithOne(ss => ss.City)
                  .HasForeignKey(ss => ss.CityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SunriseSunset>(entity =>
        {
            entity.HasKey(ss => ss.Id);
            entity.Property(ss => ss.Date).IsRequired();
        });
    }
}
