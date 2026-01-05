using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

// DTO cho thống kê voucher (CN081)
public class VoucherStatsDto
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsedCount { get; set; }
    public int? TotalUsageLimit { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public int UniqueCustomers { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public List<TopVoucherUserDto> TopUsers { get; set; } = new();
}