using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class CreatePaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public int? PaymentId { get; set; }
    public string? QrCodeUrl { get; set; } // For MoMo
}