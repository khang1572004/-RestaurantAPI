using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class TestConnectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}