using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public VendorsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<ActionResult<object>> GetAllVendors()
    {
        var vendors = await _context.Vendors
            .Select(v => new {
                v.Id,
                v.Email,
                v.BusinessName,
                v.ContactName,
                v.IsApproved,
                v.CreatedAt
            })
            .ToListAsync();
        return Ok(vendors);
    }

    [HttpPost("register")]
    public async Task<ActionResult<object>> RegisterVendor(Vendor vendor)
    {
        var existingVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == vendor.Email);
        if (existingVendor != null)
        {
            return BadRequest("Vendor with this email already exists");
        }

        vendor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vendor.PasswordHash);
        vendor.IsApproved = false;
        vendor.CreatedAt = DateTime.UtcNow;
        
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return Ok(new {
            vendor.Id,
            vendor.BusinessName,
            vendor.ContactName,
            vendor.Email,
            vendor.PhoneNumber,
            vendor.IsApproved,
            vendor.CreatedAt,
            vendor.BusinessType
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Email == request.Email);

        if (vendor == null || !BCrypt.Net.BCrypt.Verify(request.Password, vendor.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        if (!vendor.IsApproved)
        {
            return Unauthorized("Account pending approval");
        }

        return Ok(new { vendorId = vendor.Id, businessName = vendor.BusinessName });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetVendor(int id)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
        {
            return NotFound();
        }

        var locations = await _context.Locations
            .Where(l => l.VendorId == id)
            .Select(l => new {
                l.Id,
                l.Name,
                l.Description,
                l.Address,
                l.Type,
                l.PhoneNumber,
                l.Website,
                l.AverageRating,
                l.ReviewCount,
                l.ImageUrl,
                l.IsActive
            })
            .ToListAsync();

        return Ok(new {
            vendor.Id,
            vendor.BusinessName,
            vendor.ContactName,
            vendor.Email,
            vendor.PhoneNumber,
            vendor.IsApproved,
            vendor.CreatedAt,
            vendor.BusinessType,
            Locations = locations
        });
    }

    [HttpPost("{vendorId}/locations")]
    public async Task<ActionResult<object>> AddLocation(int vendorId, Location location)
    {
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null || !vendor.IsApproved)
        {
            return Unauthorized("Vendor not found or not approved");
        }

        location.VendorId = vendorId;
        location.IsActive = true; // Explicitly set to active
        location.AverageRating = 0;
        location.ReviewCount = 0;
        location.CreatedAt = DateTime.UtcNow;
        location.UpdatedAt = DateTime.UtcNow;
        
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return Ok(new {
            location.Id,
            location.Name,
            location.Description,
            location.Address,
            location.Type,
            location.Latitude,
            location.Longitude,
            location.PhoneNumber,
            location.Website,
            location.AverageRating,
            location.ReviewCount,
            location.ImageUrl,
            location.IsActive,
            location.VendorId
        });
    }

    [HttpPut("{vendorId}/locations/{locationId}")]
    public async Task<ActionResult<object>> UpdateLocation(int vendorId, int locationId, Location location)
    {
        Console.WriteLine($"UpdateLocation called - VendorId: {vendorId}, LocationId: {locationId}");
        
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null || !vendor.IsApproved)
        {
            Console.WriteLine($"Vendor not found or not approved - VendorId: {vendorId}");
            return Unauthorized("Vendor not found or not approved");
        }

        var existingLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.VendorId == vendorId);
        
        if (existingLocation == null)
        {
            Console.WriteLine($"Location not found - LocationId: {locationId}, VendorId: {vendorId}");
            
            // Debug: Check if location exists at all
            var locationById = await _context.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (locationById != null)
            {
                Console.WriteLine($"Location {locationId} exists but with VendorId: {locationById.VendorId}");
            }
            else
            {
                Console.WriteLine($"Location {locationId} does not exist in database");
            }
            
            var allLocations = await _context.Locations.Where(l => l.VendorId == vendorId).ToListAsync();
            Console.WriteLine($"Available locations for vendor {vendorId}: {string.Join(", ", allLocations.Select(l => $"ID:{l.Id}"))}");
            return NotFound(new { error = "Location not found", vendorId, locationId });
        }
        
        Console.WriteLine($"Found location: {existingLocation.Name}");

        // Update fields
        existingLocation.Name = location.Name;
        existingLocation.Description = location.Description;
        existingLocation.Address = location.Address;
        existingLocation.Type = location.Type;
        existingLocation.Latitude = location.Latitude;
        existingLocation.Longitude = location.Longitude;
        existingLocation.PhoneNumber = location.PhoneNumber;
        existingLocation.Website = location.Website;
        existingLocation.ImageUrl = location.ImageUrl;
        existingLocation.IsActive = location.IsActive;
        existingLocation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new {
            existingLocation.Id,
            existingLocation.Name,
            existingLocation.Description,
            existingLocation.Address,
            existingLocation.Type,
            existingLocation.Latitude,
            existingLocation.Longitude,
            existingLocation.PhoneNumber,
            existingLocation.Website,
            existingLocation.AverageRating,
            existingLocation.ReviewCount,
            existingLocation.ImageUrl,
            existingLocation.IsActive,
            existingLocation.VendorId
        });
    }

    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(new { error = "No image file provided" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = "Invalid file type. Only JPG, PNG, GIF, and WebP are allowed." });
        }

        // Validate file size (max 5MB)
        if (image.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { error = "File size must be less than 5MB" });
        }

        try
        {
            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return URL
            var imageUrl = $"/uploads/{fileName}";
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error uploading image: {ex.Message}" });
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}