
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class AvailableTableDto
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string? ZoneName { get; set; }
    public string TableTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsRecommended { get; set; }
}