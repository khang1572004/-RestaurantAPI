
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

    // NEW FIELD
    [Range(60, 360, ErrorMessage = "Thời gian phục vụ từ 1-6 giờ")]
    public int ServiceDurationMinutes { get; set; } = 120;

    [MaxLength(500)]
    public string? Notes { get; set; }
}