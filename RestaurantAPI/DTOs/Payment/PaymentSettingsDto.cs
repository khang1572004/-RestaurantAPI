using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class PaymentSettingsDto
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}