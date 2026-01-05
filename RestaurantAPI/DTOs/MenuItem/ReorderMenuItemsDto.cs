using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.MenuItem;

public class ReorderMenuItemsDto
{
    [Required]
    public List<MenuItemPositionDto> Items { get; set; } = new();
}