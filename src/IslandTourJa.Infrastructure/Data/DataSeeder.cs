using TripWiseJa.Domain.Entities;
using TripWiseJa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Vendors.AnyAsync())
            return; // Database already seeded

        // Create sample vendors
        var vendors = new[]
        {
            new Vendor
            {
                BusinessName = "Blue Mountain Resort",
                ContactName = "Marcus Johnson",
                Email = "marcus@bluemountain.com",
                PhoneNumber = "+1 876 555 0101",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsApproved = true
            },
            new Vendor
            {
                BusinessName = "Jerk Paradise Restaurant",
                ContactName = "Sarah Williams",
                Email = "sarah@jerkparadise.com",
                PhoneNumber = "+1 876 555 0102",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsApproved = true
            },
            new Vendor
            {
                BusinessName = "Dunn's River Adventures",
                ContactName = "Robert Brown",
                Email = "robert@dunnsriver.com",
                PhoneNumber = "+1 876 555 0103",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsApproved = true
            }
        };

        context.Vendors.AddRange(vendors);
        await context.SaveChangesAsync();

        // Create sample locations
        var locations = new[]
        {
            new Location
            {
                Name = "Blue Mountain Resort & Spa",
                Description = "Luxury resort nestled in the Blue Mountains with breathtaking views and world-class amenities.",
                Address = "Blue Mountain Peak, St. Andrew, Jamaica",
                Type = LocationType.Hotel,
                Latitude = 18.0461m,
                Longitude = -76.7319m,
                PhoneNumber = "+1 876 555 0101",
                Website = "https://bluemountainresort.com",
                ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
                AverageRating = 4.8m,
                ReviewCount = 127,
                VendorId = vendors[0].Id,
                IsActive = true
            },
            new Location
            {
                Name = "Jerk Paradise",
                Description = "Authentic Jamaican jerk cuisine in a vibrant beachside setting. Home of the best jerk chicken on the island!",
                Address = "Seven Mile Beach, Negril, Jamaica",
                Type = LocationType.Restaurant,
                Latitude = 18.3070m,
                Longitude = -78.3377m,
                PhoneNumber = "+1 876 555 0102",
                Website = "https://jerkparadise.com",
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
                AverageRating = 4.6m,
                ReviewCount = 89,
                VendorId = vendors[1].Id,
                IsActive = true
            },
            new Location
            {
                Name = "Dunn's River Falls",
                Description = "World-famous waterfall attraction offering guided climbs and natural pools for swimming.",
                Address = "Ocho Rios, St. Ann, Jamaica",
                Type = LocationType.Attraction,
                Latitude = 18.4061m,
                Longitude = -77.1519m,
                PhoneNumber = "+1 876 555 0103",
                Website = "https://dunnsriverfalls.com",
                ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
                AverageRating = 4.9m,
                ReviewCount = 203,
                VendorId = vendors[2].Id,
                IsActive = true
            }
        };

        context.Locations.AddRange(locations);
        await context.SaveChangesAsync();

        // Create sample users
        var users = new[]
        {
            new User
            {
                FirstName = "John",
                LastName = "Traveler",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            },
            new User
            {
                FirstName = "Emma",
                LastName = "Explorer",
                Email = "emma@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Create sample reviews
        var reviews = new[]
        {
            new Review
            {
                Rating = 5,
                Comment = "Absolutely stunning resort! The views are incredible and the service is top-notch.",
                UserId = users[0].Id,
                LocationId = locations[0].Id
            },
            new Review
            {
                Rating = 5,
                Comment = "Best jerk chicken I've ever had! The atmosphere is perfect and staff is friendly.",
                UserId = users[1].Id,
                LocationId = locations[1].Id
            },
            new Review
            {
                Rating = 5,
                Comment = "A must-visit attraction in Jamaica! The waterfall climb was amazing.",
                UserId = users[0].Id,
                LocationId = locations[2].Id
            }
        };

        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }
}