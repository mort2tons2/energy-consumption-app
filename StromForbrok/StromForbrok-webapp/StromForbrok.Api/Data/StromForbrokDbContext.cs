using Microsoft.EntityFrameworkCore;
using StromForbrok.Api.Domain;
using StromForbrok.Api.Domain.DegreeDays;
using StromForbrok.Api.Domain.Weather;

namespace StromForbrok.Api.Data;

public class StromForbrokDbContext(DbContextOptions<StromForbrokDbContext> options)
    : DbContext(options)
{
    public DbSet<Consumption> Consumptions { get; set; }
    public DbSet<Temperature> Temperatures { get; set; }
    public DbSet<DegreeDay> Degrees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consumption>(entity =>
        {
            entity.ToTable("Consumption");
            entity.HasKey(c => new { c.MeteringPointId, c.Timestamp });
            entity.HasIndex(c => c.Timestamp);
            entity.Property(c => c.MeteringPointId).HasMaxLength(64);
            entity.Property(c => c.Kwh).HasPrecision(18, 3);
        });

        modelBuilder.Entity<Temperature>(entity =>
        {
            entity.ToTable("Temperature");
            entity.HasKey(t => new { t.StationId, t.Timestamp });
            entity.HasIndex(t => t.Timestamp);
            entity.Property(t => t.StationId).HasMaxLength(32);
            entity.Property(t => t.Value).HasPrecision(5, 2);
        });

        modelBuilder.Entity<DegreeDay>(entity =>
        {
            entity.ToTable("DegreeDay");
            entity.Property<int>("Id");
            entity.HasKey("Id");
            entity.Property(d => d.Value).HasPrecision(10, 2);
        });
    }
}
