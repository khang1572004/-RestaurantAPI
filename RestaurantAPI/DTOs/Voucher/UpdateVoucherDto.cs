using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

// DTO để update voucher (CN077)
public class UpdateVoucherDto
{
    [Required(ErrorMessage = "Tên voucher là bắt buộc")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại giảm giá là bắt buộc")]
    public DiscountType DiscountType { get; set; }

    [Required(ErrorMessage = "Giá trị giảm là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu không hợp lệ")]
    public decimal MinOrderValue { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Số lần dùng tối đa phải lớn hơn 0")]
    public int MaxUsagePerCustomer { get; set; } = 1;

    public int? TotalUsageLimit { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}