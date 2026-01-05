
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Zone;

public class CreateZoneDto
{
    [Required(ErrorMessage = "Tên khu vực là bắt buộc")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 1000)]
    public int Position { get; set; } = 0;
}