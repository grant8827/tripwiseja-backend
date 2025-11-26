using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripWiseJa.Domain.Entities;

public class LocationImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public int LocationId { get; set; }

    // Navigation property
    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; } = null!;
}
