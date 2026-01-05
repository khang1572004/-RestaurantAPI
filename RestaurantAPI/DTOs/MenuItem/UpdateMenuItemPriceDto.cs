
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.MenuItem;

public class UpdateMenuItemPriceDto
{
    [Required(ErrorMessage = "Giá mới là bắt buộc")]
    [Range(0, 100000000, ErrorMessage = "Giá phải từ 0 đến 100,000,000")]
    public decimal NewPrice { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}