using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class UpdateOpeningHoursDto
{
    [Required(ErrorMessage = "Giờ mở cửa là bắt buộc")]
    public Dictionary<string, string> OpeningHours { get; set; } = new();
}