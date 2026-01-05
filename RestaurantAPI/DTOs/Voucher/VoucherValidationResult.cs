using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

public class VoucherValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public VoucherDto? Voucher { get; set; }
    public decimal? DiscountAmount { get; set; }
}