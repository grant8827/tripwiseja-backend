using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Vendor> Vendors => Set<Vendor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Vendor configuration
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Location configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            
            entity.HasOne(l => l.Vendor)
                  .WithMany(v => v.Locations)
                  .HasForeignKey(l => l.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Location)
                  .WithMany(l => l.Reviews)
                  .HasForeignKey(r => r.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.LocationId }).IsUnique();
        });
    }
}