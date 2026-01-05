using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class CreateBookingDto
{
    [Required(ErrorMessage = "Bàn là bắt buộc")]
    public int TableId { get; set; }

    [Required(ErrorMessage = "Ngày đặt bàn là bắt buộc")]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "Giờ đặt bàn là bắt buộc")]
    public TimeSpan BookingTime { get; set; }

    [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1-50")]
    public int NumGuests { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public string? VoucherCode { get; set; }
}