using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class TableStatusDto
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string? ZoneName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentBookingCode { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? BookingTime { get; set; }
    public int? NumGuests { get; set; }
}