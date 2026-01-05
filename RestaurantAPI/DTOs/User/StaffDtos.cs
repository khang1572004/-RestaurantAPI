using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.User;

// DTO để tạo staff mới
public class CreateStaffDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    public UserRole Role { get; set; } = UserRole.Staff;
}

// DTO để update staff
public class UpdateStaffDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    public UserRole? Role { get; set; }

    public bool? IsActive { get; set; }
}

// DTO response cho staff
public class StaffDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

// DTO để lấy danh sách staff với filter
public class StaffFilterDto
{
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; } // Search by name, email, phone
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}