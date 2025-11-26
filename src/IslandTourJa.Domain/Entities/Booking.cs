using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripWiseJa.Domain.Entities;

public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public int NumberOfGuests { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [Required]
    public int UserId { get; set; }

    [Required]
    public int LocationId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; } = null!;
}
