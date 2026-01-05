using RestaurantAPI.DTOs.User;
using RestaurantAPI.DTOs.Shift;

namespace RestaurantAPI.Services.Interfaces;

public interface IStaffService
{
    // Staff Management (CN070-CN073)
    Task<(List<StaffDto> Staff, int TotalCount)> GetAllStaffAsync(StaffFilterDto filter);
    Task<StaffDto?> GetStaffByIdAsync(int id);
    Task<(bool Success, string Message, StaffDto? Data)> CreateStaffAsync(CreateStaffDto dto);
    Task<(bool Success, string Message, StaffDto? Data)> UpdateStaffAsync(int id, UpdateStaffDto dto);
    Task<(bool Success, string Message)> DeleteStaffAsync(int id);

    // Shift Management (CN074-CN075, CN042)
    Task<(List<ShiftDto> Shifts, int TotalCount)> GetAllShiftsAsync(ShiftFilterDto filter);
    Task<ShiftDto?> GetShiftByIdAsync(int id);
    Task<List<ShiftDto>> GetStaffShiftsAsync(int staffId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ShiftCalendarDto>> GetShiftCalendarAsync(DateTime fromDate, DateTime toDate);
    Task<(bool Success, string Message, ShiftDto? Data)> CreateShiftAsync(CreateShiftDto dto);
    Task<(bool Success, string Message, ShiftDto? Data)> UpdateShiftAsync(int id, UpdateShiftDto dto);
    Task<(bool Success, string Message)> DeleteShiftAsync(int id);

    // Helper methods
    Task<bool> IsStaffAvailableAsync(int staffId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null);
}