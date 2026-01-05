using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

public class VoucherDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public int MaxUsagePerCustomer { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; } // Computed: active + not expired + has usage left
}