using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

// DTO để filter vouchers (CN080)
public class VoucherFilterDto
{
    public bool? IsActive { get; set; }
    public bool? IsExpired { get; set; }
    public string? SearchTerm { get; set; } // Search by code, name
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}