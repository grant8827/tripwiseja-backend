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

    [HttpPost("register")]
    public async Task<ActionResult<object>> RegisterVendor(Vendor vendor)
    {
        // Check if email already exists
        var existingVendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Email == vendor.Email);
        
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

    [HttpPost("{vendorId}/locations/{locationId}/images")]
    public async Task<ActionResult<object>> AddLocationImage(int vendorId, int locationId, [FromForm] IFormFile image, [FromForm] string? caption)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.VendorId == vendorId);

        if (location == null)
        {
            return NotFound("Location not found or access denied");
        }

        if (image == null || image.Length == 0)
        {
            return BadRequest("No image provided");
        }

        // Check file size (max 10MB)
        if (image.Length > 10 * 1024 * 1024)
        {
            return BadRequest("Image size must be less than 10MB");
        }

        // Check file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
        {
            return BadRequest("Only image files are allowed (JPEG, PNG, GIF, WebP)");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/{uniqueFileName}";

        // Get the current max display order
        var maxOrder = await _context.LocationImages
            .Where(li => li.LocationId == locationId)
            .MaxAsync(li => (int?)li.DisplayOrder) ?? -1;

        var locationImage = new LocationImage
        {
            LocationId = locationId,
            ImageUrl = imageUrl,
            Caption = caption,
            DisplayOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.LocationImages.Add(locationImage);
        await _context.SaveChangesAsync();

        return Ok(new {
            locationImage.Id,
            locationImage.ImageUrl,
            locationImage.Caption,
            locationImage.DisplayOrder,
            locationImage.CreatedAt
        });
    }

    [HttpGet("{vendorId}/locations/{locationId}/images")]
    public async Task<ActionResult<object>> GetLocationImages(int vendorId, int locationId)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.VendorId == vendorId);

        if (location == null)
        {
            return NotFound("Location not found or access denied");
        }

        var images = await _context.LocationImages
            .Where(li => li.LocationId == locationId)
            .OrderBy(li => li.DisplayOrder)
            .Select(li => new {
                li.Id,
                li.ImageUrl,
                li.Caption,
                li.DisplayOrder,
                li.CreatedAt
            })
            .ToListAsync();

        return Ok(images);
    }

    [HttpDelete("{vendorId}/locations/{locationId}/images/{imageId}")]
    public async Task<ActionResult> DeleteLocationImage(int vendorId, int locationId, int imageId)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.VendorId == vendorId);

        if (location == null)
        {
            return NotFound("Location not found or access denied");
        }

        var image = await _context.LocationImages
            .FirstOrDefaultAsync(li => li.Id == imageId && li.LocationId == locationId);

        if (image == null)
        {
            return NotFound("Image not found");
        }

        // Delete physical file
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.LocationImages.Remove(image);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Image deleted successfully" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}