public class VoucherUsage
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public int CustomerId { get; set; }
    public int? BookingId { get; set; }
    public int? OrderId { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation - ✅ Thêm null! hoặc ?
    public Voucher Voucher { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Booking? Booking { get; set; } // Nullable vì BookingId nullable
    public Order? Order { get; set; } // Nullable vì OrderId nullable
}