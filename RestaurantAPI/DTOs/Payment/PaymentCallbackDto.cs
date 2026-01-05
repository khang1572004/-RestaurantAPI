using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class PaymentCallbackDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? BookingCode { get; set; }
    public decimal? Amount { get; set; }
    public string? TransactionId { get; set; }
}