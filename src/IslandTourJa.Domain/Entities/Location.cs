using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TripWiseJa.Domain.Enums;

namespace TripWiseJa.Domain.Entities;

public class Location
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public LocationType Type { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? Website { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0;

    public int ReviewCount { get; set; } = 0;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int? VendorId { get; set; }

    // Navigation properties
    [ForeignKey("VendorId")]
    public virtual Vendor? Vendor { get; set; }
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<LocationImage> Images { get; set; } = new List<LocationImage>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}