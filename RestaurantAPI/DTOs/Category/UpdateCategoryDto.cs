using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Category;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 1000)]
    public int Position { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}