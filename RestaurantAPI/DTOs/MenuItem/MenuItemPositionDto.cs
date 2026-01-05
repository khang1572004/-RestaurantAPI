using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.MenuItem;

public class MenuItemPositionDto
{
    public int Id { get; set; }
    public int Position { get; set; }
}