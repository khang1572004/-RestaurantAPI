using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.MenuItem;

public class UpdateMenuItemAvailabilityDto
{
    [Required]
    public bool IsAvailable { get; set; }
}