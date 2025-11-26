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

    // POST: api/bookings
    [HttpPost]
    public async Task<ActionResult<object>> CreateBooking([FromBody] BookingDto bookingDto)
    {
        // Verify user exists
        var user = await _context.Users.FindAsync(bookingDto.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Verify location exists
        var location = await _context.Locations.FindAsync(bookingDto.LocationId);
        if (location == null)
        {
            return NotFound("Location not found");
        }

        // Validate dates
        if (bookingDto.CheckInDate >= bookingDto.CheckOutDate)
        {
            return BadRequest("Check-out date must be after check-in date");
        }

        if (bookingDto.CheckInDate < DateTime.UtcNow.Date)
        {
            return BadRequest("Check-in date cannot be in the past");
        }

        var booking = new Booking
        {
            UserId = bookingDto.UserId,
            LocationId = bookingDto.LocationId,
            CheckInDate = bookingDto.CheckInDate,
            CheckOutDate = bookingDto.CheckOutDate,
            NumberOfGuests = bookingDto.NumberOfGuests,
            SpecialRequests = bookingDto.SpecialRequests,
            Status = "Pending",
            TotalPrice = bookingDto.TotalPrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            booking.Id,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.NumberOfGuests,
            booking.SpecialRequests,
            booking.Status,
            booking.TotalPrice,
            booking.CreatedAt,
            Location = new
            {
                location.Id,
                location.Name,
                location.Address,
                location.ImageUrl
            }
        });
    }

    // GET: api/bookings/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<object>> GetUserBookings(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Location)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.CheckInDate,
                b.CheckOutDate,
                b.NumberOfGuests,
                b.SpecialRequests,
                b.Status,
                b.TotalPrice,
                b.CreatedAt,
                b.UpdatedAt,
                Location = new
                {
                    b.Location.Id,
                    b.Location.Name,
                    b.Location.Address,
                    b.Location.Type,
                    b.Location.ImageUrl,
                    b.Location.PhoneNumber
                }
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // GET: api/bookings/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Where(b => b.Id == id)
            .Include(b => b.Location)
            .Include(b => b.User)
            .Select(b => new
            {
                b.Id,
                b.CheckInDate,
                b.CheckOutDate,
                b.NumberOfGuests,
                b.SpecialRequests,
                b.Status,
                b.TotalPrice,
                b.CreatedAt,
                b.UpdatedAt,
                Location = new
                {
                    b.Location.Id,
                    b.Location.Name,
                    b.Location.Address,
                    b.Location.Type,
                    b.Location.ImageUrl,
                    b.Location.PhoneNumber,
                    b.Location.Website
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
            return NotFound("Booking not found");
        }

        return Ok(booking);
    }

    // PUT: api/bookings/{id}/cancel
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult> CancelBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound("Booking not found");
        }

        if (booking.Status == "Cancelled" || booking.Status == "Completed")
        {
            return BadRequest($"Cannot cancel a booking with status: {booking.Status}");
        }

        booking.Status = "Cancelled";
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Booking cancelled successfully", booking.Status });
    }

    // PUT: api/bookings/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateBooking(int id, [FromBody] BookingUpdateDto updateDto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound("Booking not found");
        }

        if (booking.Status == "Cancelled" || booking.Status == "Completed")
        {
            return BadRequest($"Cannot update a booking with status: {booking.Status}");
        }

        // Validate dates if provided
        var checkInDate = updateDto.CheckInDate ?? booking.CheckInDate;
        var checkOutDate = updateDto.CheckOutDate ?? booking.CheckOutDate;

        if (checkInDate >= checkOutDate)
        {
            return BadRequest("Check-out date must be after check-in date");
        }

        if (checkInDate < DateTime.UtcNow.Date)
        {
            return BadRequest("Check-in date cannot be in the past");
        }

        booking.CheckInDate = checkInDate;
        booking.CheckOutDate = checkOutDate;
        booking.NumberOfGuests = updateDto.NumberOfGuests ?? booking.NumberOfGuests;
        booking.SpecialRequests = updateDto.SpecialRequests ?? booking.SpecialRequests;
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
            booking.TotalPrice,
            booking.UpdatedAt
        });
    }
}

public class BookingDto
{
    public int UserId { get; set; }
    public int LocationId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string? SpecialRequests { get; set; }
    public decimal? TotalPrice { get; set; }
}

public class BookingUpdateDto
{
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? NumberOfGuests { get; set; }
    public string? SpecialRequests { get; set; }
}
