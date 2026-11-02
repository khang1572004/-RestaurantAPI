using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? Phone { get; set; } // ✅ Nullable

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsEmailVerified { get; set; } = false;

    // THÊM DẤU ? ĐỂ NULLABLE
    [MaxLength(100)]
    public string? EmailVerificationToken { get; set; }

    // THÊM DẤU ? ĐỂ NULLABLE
    [MaxLength(100)]
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetExpiry { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(128)]
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    public ICollection<RefundRequest> ProcessedRefunds { get; set; } = new List<RefundRequest>();
    public ICollection<MenuItemPriceHistory> PriceChanges { get; set; } = new List<MenuItemPriceHistory>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
    public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
}
