using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

public class ValidateVoucherDto
{
    [Required(ErrorMessage = "Mã voucher là bắt buộc")]
    public string Code { get; set; } = string.Empty;

    public decimal? OrderValue { get; set; }
}