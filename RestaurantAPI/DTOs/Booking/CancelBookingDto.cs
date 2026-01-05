using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class CancelBookingDto
{
    [Required(ErrorMessage = "Lý do hủy là bắt buộc")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
