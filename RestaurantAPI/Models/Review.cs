using System.ComponentModel.DataAnnotations;

public class Review
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int BookingId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Customer { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}