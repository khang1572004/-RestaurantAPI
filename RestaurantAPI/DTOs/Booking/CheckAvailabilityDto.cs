using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class CheckAvailabilityDto
{
    [Required(ErrorMessage = "Ngày đặt bàn là bắt buộc")]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "Giờ đặt bàn là bắt buộc")]
    public TimeSpan BookingTime { get; set; }

    [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1-50")]
    public int NumGuests { get; set; }

    // NEW FIELD
    [Range(60, 360, ErrorMessage = "Thời gian phục vụ từ 1-6 giờ")]
    public int ServiceDurationMinutes { get; set; } = 120;
}