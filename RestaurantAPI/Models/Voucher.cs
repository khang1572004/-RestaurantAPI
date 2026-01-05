using System.ComponentModel.DataAnnotations;

public class Voucher
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    public decimal DiscountValue { get; set; }

    public decimal MinOrderValue { get; set; } = 0;
    public int MaxUsagePerCustomer { get; set; } = 1;
    public int? TotalUsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

