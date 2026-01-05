using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class UpdateRestaurantSettingsDto
{
    [Required(ErrorMessage = "Tên nhà hàng là bắt buộc")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
}