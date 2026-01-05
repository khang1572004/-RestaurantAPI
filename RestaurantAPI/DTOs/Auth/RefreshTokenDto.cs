using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token là bắt buộc")]
    public string RefreshToken { get; set; } = string.Empty;
}