using System.ComponentModel.DataAnnotations;
using TripWiseJa.Domain.Enums;

namespace TripWiseJa.Domain.Entities;

public class Vendor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public LocationType BusinessType { get; set; }

    // Navigation properties
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}