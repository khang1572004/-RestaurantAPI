
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class UpdateBookingDto
{
    [Required]
    public int TableId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public TimeSpan BookingTime { get; set; }

    [Required]
    [Range(1, 50)]
    public int NumGuests { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}