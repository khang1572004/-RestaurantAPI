using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class UpdatePaymentSettingsDto
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ReturnUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}