using System.ComponentModel.DataAnnotations;

public class Order
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public int TableId { get; set; }
    public int? StaffId { get; set; }

    [Required]
    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; } = 0;
    public int? VoucherId { get; set; }
    public decimal DepositAmount { get; set; } = 0;

    [Required]
    public decimal FinalAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    // Navigation - ✅ Thêm null! hoặc ?
    public Booking? Booking { get; set; } // Nullable vì BookingId nullable
    public Table Table { get; set; } = null!;
    public User? Staff { get; set; } // Nullable vì StaffId nullable
    public Voucher? Voucher { get; set; } // Nullable vì VoucherId nullable
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

