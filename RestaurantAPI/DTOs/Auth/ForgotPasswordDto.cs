using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}