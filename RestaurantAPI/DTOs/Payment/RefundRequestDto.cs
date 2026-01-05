using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class RefundRequestDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int PaymentId { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedByName { get; set; }
    public string? AdminNote { get; set; }
}