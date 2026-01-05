using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class ProcessRefundDto
{
    [MaxLength(500)]
    public string? AdminNote { get; set; }
}