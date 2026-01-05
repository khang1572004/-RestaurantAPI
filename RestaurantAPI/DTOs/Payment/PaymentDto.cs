using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public string? BookingCode { get; set; }
    public int? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}