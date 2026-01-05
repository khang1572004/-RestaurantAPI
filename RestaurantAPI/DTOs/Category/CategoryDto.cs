using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
    public int MenuItemsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}