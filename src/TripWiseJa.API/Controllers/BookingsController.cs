using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public BookingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    public class CreateBookingDto
    {
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string? SpecialRequests { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class UpdateBookingDto
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? SpecialRequests { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateBooking([FromBody] CreateBookingDto dto)
    {
        // Validate user exists
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        // Validate location exists
        var location = await _context.Locations.FindAsync(dto.LocationId);
        if (location == null)
        {
            return BadRequest("Location not found");
        }

        // Validate dates
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            return BadRequest("Check-out date must be after check-in date");
        }

        if (dto.CheckInDate < DateTime.Today)
        {
            return BadRequest("Check-in date cannot be in the past");
        }

        var booking = new Booking
        {
            UserId = dto.UserId,
            LocationId = dto.LocationId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            NumberOfGuests = dto.NumberOfGuests,
            SpecialRequests = dto.SpecialRequests,
            TotalPrice = dto.TotalPrice,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            booking.Id,
            booking.UserId,
            booking.LocationId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.NumberOfGuests,
            booking.SpecialRequests,
            booking.TotalPrice,
            booking.Status,
            booking.CreatedAt,
            Message = "Booking created successfully"
        });
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<object[]>> GetUserBookings(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Location)
            .Select(b => new
            {
                b.Id,
                b.CheckInDate,
                b.CheckOutDate,
                b.NumberOfGuests,
                b.SpecialRequests,
                b.TotalPrice,
                b.Status,
                b.CreatedAt,
                Location = new
                {
                    b.Location.Id,
                    b.Location.Name,
                    b.Location.Address,
                    b.Location.ImageUrl
                }
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToArrayAsync();

        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Location)
            .Include(b => b.User)
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.CheckInDate,
                b.CheckOutDate,
                b.NumberOfGuests,
                b.SpecialRequests,
                b.TotalPrice,
                b.Status,
                b.CreatedAt,
                Location = new
                {
                    b.Location.Id,
                    b.Location.Name,
                    b.Location.Address,
                    b.Location.ImageUrl
                },
                User = new
                {
                    b.User.Id,
                    b.User.FirstName,
                    b.User.LastName,
                    b.User.Email
                }
            })
            .FirstOrDefaultAsync();

        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<object>> CancelBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.Status == "Cancelled")
        {
            return BadRequest("Booking is already cancelled");
        }

        booking.Status = "Cancelled";
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            booking.Id,
            booking.Status,
            Message = "Booking cancelled successfully"
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateBooking(int id, [FromBody] UpdateBookingDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.Status == "Cancelled")
        {
            return BadRequest("Cannot update a cancelled booking");
        }

        if (dto.CheckInDate.HasValue)
        {
            if (dto.CheckInDate.Value < DateTime.Today)
            {
                return BadRequest("Check-in date cannot be in the past");
            }
            booking.CheckInDate = dto.CheckInDate.Value;
        }

        if (dto.CheckOutDate.HasValue)
        {
            booking.CheckOutDate = dto.CheckOutDate.Value;
        }

        if (booking.CheckInDate >= booking.CheckOutDate)
        {
            return BadRequest("Check-out date must be after check-in date");
        }

        if (dto.NumberOfGuests.HasValue)
        {
            booking.NumberOfGuests = dto.NumberOfGuests.Value;
        }

        if (dto.SpecialRequests != null)
        {
            booking.SpecialRequests = dto.SpecialRequests;
        }

        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            booking.Id,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.NumberOfGuests,
            booking.SpecialRequests,
            booking.Status,
            Message = "Booking updated successfully"
        });
    }
}