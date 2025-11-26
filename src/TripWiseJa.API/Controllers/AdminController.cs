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
}