using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReviewsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("location/{locationId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetLocationReviews(int locationId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.LocationId == locationId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.UserId,
                r.LocationId,
                User = new {
                    r.User.Id,
                    r.User.FirstName,
                    r.User.LastName,
                    r.User.Email
                }
            })
            .ToListAsync();

        return Ok(reviews);
    }

    public class AnonymousReviewDto
    {
        public int LocationId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [HttpPost("anonymous")]
    public async Task<ActionResult<object>> PostAnonymousReview([FromBody] AnonymousReviewDto dto)
    {
        try
        {
            Console.WriteLine($"[Anonymous Review] Received: LocationId={dto.LocationId}, Rating={dto.Rating}, Email={dto.Email}");
            
            // Validate that the location exists
            var location = await _context.Locations.FindAsync(dto.LocationId);
            if (location == null)
            {
                Console.WriteLine($"[Anonymous Review] Location not found: {dto.LocationId}");
                return NotFound("Location not found");
            }

            Console.WriteLine($"[Anonymous Review] Location found: {location.Name}");

            // Create or find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                Console.WriteLine($"[Anonymous Review] Creating new user: {dto.Email}");
                user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PasswordHash = string.Empty, // No password for guest users
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Anonymous Review] User created with ID: {user.Id}");
            }
            else
            {
                Console.WriteLine($"[Anonymous Review] User found: {user.FirstName} {user.LastName} (ID: {user.Id})");
            }

            // Check if this email already reviewed this location
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.LocationId == dto.LocationId);

            if (existingReview != null)
            {
                Console.WriteLine($"[Anonymous Review] Duplicate review detected for email: {dto.Email}");
                return BadRequest("This email has already reviewed this location");
            }

            Console.WriteLine($"[Anonymous Review] Creating review...");
            var review = new Review
            {
                UserId = user.Id,
                LocationId = dto.LocationId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[Anonymous Review] Review created with ID: {review.Id}");

            // Update location's average rating and review count
            await UpdateLocationRating(dto.LocationId);

            // Fetch the created review with user data
            var createdReview = await _context.Reviews
                .Where(r => r.Id == review.Id)
                .Include(r => r.User)
                .Select(r => new {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    r.UserId,
                    r.LocationId,
                    User = new {
                        r.User.Id,
                        r.User.FirstName,
                        r.User.LastName,
                        r.User.Email
                    }
                })
                .FirstOrDefaultAsync();

            Console.WriteLine($"[Anonymous Review] Success! Returning review data");
            return CreatedAtAction(nameof(GetLocationReviews), new { locationId = dto.LocationId }, createdReview);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Anonymous Review] ERROR: {ex.Message}");
            Console.WriteLine($"[Anonymous Review] Stack: {ex.StackTrace}");
            return StatusCode(500, $"Failed to submit review: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> PostReview([FromBody] Review review)
    {
        // Validate that the location exists
        var location = await _context.Locations.FindAsync(review.LocationId);
        if (location == null)
        {
            return NotFound("Location not found");
        }

        // Validate that the user exists
        var user = await _context.Users.FindAsync(review.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Check if user already reviewed this location
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == review.UserId && r.LocationId == review.LocationId);

        if (existingReview != null)
        {
            return BadRequest("You have already reviewed this location");
        }

        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update location's average rating and review count
        await UpdateLocationRating(review.LocationId);

        // Fetch the created review with user data
        var createdReview = await _context.Reviews
            .Where(r => r.Id == review.Id)
            .Include(r => r.User)
            .Select(r => new {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.UserId,
                r.LocationId,
                User = new {
                    r.User.Id,
                    r.User.FirstName,
                    r.User.LastName,
                    r.User.Email
                }
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetLocationReviews), new { locationId = review.LocationId }, createdReview);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] Review review)
    {
        if (id != review.Id)
        {
            return BadRequest();
        }

        var existingReview = await _context.Reviews.FindAsync(id);
        if (existingReview == null)
        {
            return NotFound();
        }

        existingReview.Rating = review.Rating;
        existingReview.Comment = review.Comment;
        existingReview.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update location's average rating
        await UpdateLocationRating(existingReview.LocationId);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var locationId = review.LocationId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        // Update location's average rating
        await UpdateLocationRating(locationId);

        return NoContent();
    }

    private async Task UpdateLocationRating(int locationId)
    {
        var location = await _context.Locations.FindAsync(locationId);
        if (location == null) return;

        var reviews = await _context.Reviews
            .Where(r => r.LocationId == locationId)
            .ToListAsync();

        if (reviews.Any())
        {
            location.AverageRating = (decimal)reviews.Average(r => r.Rating);
            location.ReviewCount = reviews.Count;
        }
        else
        {
            location.AverageRating = 0;
            location.ReviewCount = 0;
        }

        await _context.SaveChangesAsync();
    }
}
