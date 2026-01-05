using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Shift;

// DTO để tạo shift mới
public class CreateShiftDto
{
    [Required(ErrorMessage = "Staff ID là bắt buộc")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Ngày làm việc là bắt buộc")]
    public DateTime ShiftDate { get; set; }

    [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
    public TimeSpan EndTime { get; set; }
}

// DTO để update shift
public class UpdateShiftDto
{
    [Required(ErrorMessage = "Ngày làm việc là bắt buộc")]
    public DateTime ShiftDate { get; set; }

    [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
    public TimeSpan EndTime { get; set; }
}

// DTO response cho shift
public class ShiftDto
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? StaffEmail { get; set; }
    public DateTime ShiftDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Duration { get; set; } = string.Empty; // Computed: "8 hours"
    public DateTime CreatedAt { get; set; }
}

// DTO để filter shifts
public class ShiftFilterDto
{
    public int? StaffId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// DTO cho calendar view
public class ShiftCalendarDto
{
    public DateTime Date { get; set; }
    public List<ShiftDto> Shifts { get; set; } = new();
    public int TotalStaff { get; set; }
}