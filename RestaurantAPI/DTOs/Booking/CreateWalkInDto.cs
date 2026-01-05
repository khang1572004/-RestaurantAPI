using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class CreateWalkInDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
    [Range(1, 50)]
    public int NumGuests { get; set; }

    [Required(ErrorMessage = "Bàn là bắt buộc")]
    public int TableId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}