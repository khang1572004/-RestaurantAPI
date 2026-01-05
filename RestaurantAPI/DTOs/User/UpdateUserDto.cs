using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.User;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? Phone { get; set; }
}