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

    public class CreateReviewDto
    {
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class CreateAnonymousReviewDto
    {
        public int LocationId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    [HttpGet("location/{locationId}")]
    public async Task<ActionResult<object[]>> GetLocationReviews(int locationId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.LocationId == locationId)
            .Include(r => r.User)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                User = new
                {
                    r.User.Id,
                    r.User.FirstName,
                    r.User.LastName
                }
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToArrayAsync();

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<ActionResult<object>> SubmitReview([FromBody] CreateReviewDto dto)
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

        // Check if user already reviewed this location
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.LocationId == dto.LocationId);

        if (existingReview != null)
        {
            return BadRequest("You have already reviewed this location");
        }

        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5");
        }

        var review = new Review
        {
            UserId = dto.UserId,
            LocationId = dto.LocationId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update location's average rating
        await UpdateLocationRating(dto.LocationId);

        return Ok(new
        {
            review.Id,
            review.UserId,
            review.LocationId,
            review.Rating,
            review.Comment,
            review.CreatedAt,
            Message = "Review submitted successfully"
        });
    }

    [HttpPost("anonymous")]
    public async Task<ActionResult<object>> SubmitAnonymousReview([FromBody] CreateAnonymousReviewDto dto)
    {
        // Validate location exists
        var location = await _context.Locations.FindAsync(dto.LocationId);
        if (location == null)
        {
            return BadRequest("Location not found");
        }

        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5");
        }

        // Create or find anonymous user
        var anonymousUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (anonymousUser == null)
        {
            // Create anonymous user
            anonymousUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = "", // Anonymous users don't have passwords
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(anonymousUser);
            await _context.SaveChangesAsync();
        }

        // Check if this email already reviewed this location
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == anonymousUser.Id && r.LocationId == dto.LocationId);

        if (existingReview != null)
        {
            return BadRequest("This email has already reviewed this location");
        }

        var review = new Review
        {
            UserId = anonymousUser.Id,
            LocationId = dto.LocationId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update location's average rating
        await UpdateLocationRating(dto.LocationId);

        return Ok(new
        {
            review.Id,
            review.LocationId,
            review.Rating,
            review.Comment,
            review.CreatedAt,
            Message = "Anonymous review submitted successfully"
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        // Validate user owns this review
        if (review.UserId != dto.UserId)
        {
            return Forbid("You can only update your own reviews");
        }

        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5");
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update location's average rating
        await UpdateLocationRating(review.LocationId);

        return Ok(new
        {
            review.Id,
            review.Rating,
            review.Comment,
            review.UpdatedAt,
            Message = "Review updated successfully"
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(int id)
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

        return Ok(new { Message = "Review deleted successfully" });
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

        location.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}