using TripWiseJa.Application.Interfaces;
using TripWiseJa.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace TripWiseJa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UsersController(IApplicationDbContext context)
    {
        _context = context;
    }

    public class UserRegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            Console.WriteLine($"📝 Registration attempt for: {dto.Email}");
            
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                Console.WriteLine($"❌ User already exists: {dto.Email}");
                return BadRequest("User with this email already exists");
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            Console.WriteLine($"🔐 Password hashed for: {dto.Email}");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            Console.WriteLine($"➕ User added to context: {dto.Email}");
            
            var result = await _context.SaveChangesAsync();
            Console.WriteLine($"💾 SaveChanges result: {result} rows affected");
            Console.WriteLine($"✅ User registered successfully: {user.Id} - {dto.Email}");

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Message = "Registration successful"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Registration error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, $"Registration failed: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] UserLoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password");
        }

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            Message = "Login successful"
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
