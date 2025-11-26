using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using TripWiseJa.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public LocationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetLocations([FromQuery] string? type = null)
    {
        var query = _context.Locations.Where(l => l.IsActive);

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<LocationType>(type, true, out var locationType))
            {
                query = query.Where(l => l.Type == locationType);
            }
        }

        var locations = await query
            .Select(l => new {
                l.Id,
                l.Name,
                l.Description,
                l.Address,
                l.Type,
                l.Latitude,
                l.Longitude,
                l.PhoneNumber,
                l.Website,
                l.AverageRating,
                l.ReviewCount,
                l.ImageUrl,
                l.IsActive,
                l.VendorId
            })
            .ToListAsync();

        var sortedLocations = locations.OrderByDescending(l => l.AverageRating).ToList();
        return Ok(sortedLocations);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<object>>> GetFeaturedLocations()
    {
        var locations = await _context.Locations
            .Where(l => l.IsActive)
            .Select(l => new {
                l.Id,
                l.Name,
                l.Description,
                l.Address,
                l.Type,
                l.Latitude,
                l.Longitude,
                l.PhoneNumber,
                l.Website,
                l.AverageRating,
                l.ReviewCount,
                l.ImageUrl,
                l.IsActive,
                l.VendorId
            })
            .ToListAsync();

        var featuredLocations = locations
            .Where(l => l.AverageRating >= 4.0m)
            .OrderByDescending(l => l.AverageRating)
            .Take(6)
            .ToList();

        return Ok(featuredLocations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetLocation(int id)
    {
        var location = await _context.Locations
            .Where(l => l.Id == id)
            .Select(l => new {
                l.Id,
                l.Name,
                l.Description,
                l.Address,
                l.Type,
                l.Latitude,
                l.Longitude,
                l.PhoneNumber,
                l.Website,
                l.AverageRating,
                l.ReviewCount,
                l.ImageUrl,
                l.IsActive,
                l.VendorId
            })
            .FirstOrDefaultAsync();

        if (location == null)
        {
            return NotFound();
        }

        return Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<Location>> PostLocation(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }
}