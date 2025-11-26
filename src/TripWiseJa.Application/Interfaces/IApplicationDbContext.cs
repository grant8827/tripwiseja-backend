using TripWiseJa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Location> Locations { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Vendor> Vendors { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}