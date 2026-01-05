using System.ComponentModel.DataAnnotations;

public class RefundRequest
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int PaymentId { get; set; }

    [Required]
    public decimal RefundAmount { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; } 

    [Required]
    public RefundStatus Status { get; set; } = RefundStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }

    [MaxLength(500)]
    public string? AdminNote { get; set; } 

    // Navigation
    public Booking Booking { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
}
