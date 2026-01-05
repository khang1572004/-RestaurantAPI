using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Voucher;

public class TopVoucherUserDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalSaved { get; set; }
}