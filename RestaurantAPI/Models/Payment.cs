using System.ComponentModel.DataAnnotations;

public class Payment
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public int? OrderId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentType PaymentType { get; set; }

    [Required]
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;

    [MaxLength(100)]
    public string? TransactionId { get; set; } 

    public string? PaymentResponse { get; set; } 

    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Booking? Booking { get; set; }
    public Order? Order { get; set; }
    public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}

