using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class CheckInDto
{
    [Required(ErrorMessage = "Mã đặt bàn là bắt buộc")]
    public string BookingCode { get; set; } = string.Empty;
}