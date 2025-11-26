using TripWiseJa.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AdminController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("vendors/pending")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingVendors()
    {
        var vendors = await _context.Vendors
            .Where(v => !v.IsApproved)
            .Select(v => new {
                v.Id,
                v.BusinessName,
                v.ContactName,
                v.Email,
                v.PhoneNumber,
                v.BusinessType,
                v.CreatedAt
            })
            .ToListAsync();

        return Ok(vendors);
    }

    [HttpPut("vendors/{id}/approve")]
    public async Task<ActionResult> ApproveVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        vendor.IsApproved = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Vendor approved successfully" });
    }

    [HttpDelete("vendors/{id}")]
    public async Task<ActionResult> RejectVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Vendor rejected and removed" });
    }

    [HttpGet("vendors/approved")]
    public async Task<ActionResult<IEnumerable<object>>> GetApprovedVendors()
    {
        var vendors = await _context.Vendors
            .Where(v => v.IsApproved)
            .Select(v => new {
                v.Id,
                v.BusinessName,
                v.ContactName,
                v.Email,
                v.PhoneNumber,
                v.BusinessType,
                v.CreatedAt,
                v.IsApproved
            })
            .ToListAsync();

        return Ok(vendors);
    }

    [HttpPut("vendors/{id}/suspend")]
    public async Task<ActionResult> SuspendVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        vendor.IsApproved = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Vendor suspended successfully" });
    }

    [HttpPut("vendors/{id}")]
    public async Task<ActionResult> UpdateVendor(int id, [FromBody] VendorUpdateDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        vendor.BusinessName = dto.BusinessName ?? vendor.BusinessName;
        vendor.ContactName = dto.ContactName ?? vendor.ContactName;
        vendor.Email = dto.Email ?? vendor.Email;
        vendor.PhoneNumber = dto.PhoneNumber ?? vendor.PhoneNumber;
        if (dto.BusinessType.HasValue)
        {
            vendor.BusinessType = dto.BusinessType.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Vendor updated successfully" });
    }

    public class VendorUpdateDto
    {
        public string? BusinessName { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? BusinessType { get; set; }
    }
}