using System.ComponentModel.DataAnnotations;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int TableId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public TimeSpan BookingTime { get; set; }

    [Required]
    public int NumGuests { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; } // ✅ Nullable

    [Required, MaxLength(20)]
    public string BookingCode { get; set; } = string.Empty;

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public int? VoucherId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; } 

    // NEW
    public int ServiceDurationMinutes { get; set; } = 120; // Default 2h
    public bool IsLateCheckIn { get; set; } = false;
    public bool DepositForfeited { get; set; } = false;

    // Navigation
    public User Customer { get; set; } = null!;
    public Table Table { get; set; } = null!;
    public Voucher? Voucher { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Order? Order { get; set; }
    public Review? Review { get; set; }
    public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}

