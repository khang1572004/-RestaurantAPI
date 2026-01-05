using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.MenuItem;

public class UpdateMenuItemDto
{
    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, 100000000, ErrorMessage = "Giá phải từ 0 đến 100,000,000")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    [Range(0, 1000)]
    public int Position { get; set; } = 0;
}