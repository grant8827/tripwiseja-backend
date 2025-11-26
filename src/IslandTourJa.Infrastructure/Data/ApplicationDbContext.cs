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
    public DbSet<LocationImage> LocationImages => Set<LocationImage>();
    public DbSet<Booking> Bookings => Set<Booking>();

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

        // LocationImage configuration
        modelBuilder.Entity<LocationImage>(entity =>
        {
            entity.HasOne(li => li.Location)
                  .WithMany(l => l.Images)
                  .HasForeignKey(li => li.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.LocationId, e.DisplayOrder });
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Location)
                  .WithMany(l => l.Bookings)
                  .HasForeignKey(b => b.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.LocationId, e.CheckInDate });
        });
    }
}