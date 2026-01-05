using System.ComponentModel.DataAnnotations;

public class StaffShift
{
    public int Id { get; set; }
    public int StaffId { get; set; }

    [Required]
    public DateTime ShiftDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation - ✅ Thêm null!
    public User Staff { get; set; } = null!;
}