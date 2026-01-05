using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.User;
using RestaurantAPI.DTOs.Shift;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StaffService> _logger;

    public StaffService(ApplicationDbContext context, ILogger<StaffService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // STAFF MANAGEMENT
    // ============================================

    // CN073: Get all staff với filter
    public async Task<(List<StaffDto> Staff, int TotalCount)> GetAllStaffAsync(StaffFilterDto filter)
    {
        var query = _context.Users
            .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
            .AsQueryable();

        // Apply filters
        if (filter.Role.HasValue)
        {
            query = query.Where(u => u.Role == filter.Role.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                (u.Phone != null && u.Phone.Contains(searchTerm))
            );
        }

        var totalCount = await query.CountAsync();

        var staff = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new StaffDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return (staff, totalCount);
    }

    public async Task<StaffDto?> GetStaffByIdAsync(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && (u.Role == UserRole.Staff || u.Role == UserRole.Admin))
            .Select(u => new StaffDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync();

        return user;
    }

    // CN070: Tạo staff mới
    public async Task<(bool Success, string Message, StaffDto? Data)> CreateStaffAsync(CreateStaffDto dto)
    {
        // Check email exists
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return (false, "Email đã được sử dụng", null);
        }

        // Check phone exists
        if (!string.IsNullOrEmpty(dto.Phone))
        {
            var phoneExists = await _context.Users.AnyAsync(u => u.Phone == dto.Phone);
            if (phoneExists)
            {
                return (false, "Số điện thoại đã được sử dụng", null);
            }
        }

        // Validate role
        if (dto.Role != UserRole.Staff && dto.Role != UserRole.Admin)
        {
            return (false, "Vai trò không hợp lệ. Chỉ có thể tạo Staff hoặc Admin", null);
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            FullName = dto.FullName,
            Phone = dto.Phone,
            Role = dto.Role,
            IsActive = true,
            IsEmailVerified = true, // Staff accounts auto-verified
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new staff account: {Email}", dto.Email);

        var staffDto = new StaffDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };

        return (true, "Tạo tài khoản nhân viên thành công", staffDto);
    }

    // CN072: Update staff
    public async Task<(bool Success, string Message, StaffDto? Data)> UpdateStaffAsync(int id, UpdateStaffDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return (false, "Nhân viên không tồn tại", null);
        }

        if (user.Role != UserRole.Staff && user.Role != UserRole.Admin)
        {
            return (false, "Không thể cập nhật tài khoản này", null);
        }

        // Check phone uniqueness
        if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != user.Phone)
        {
            var phoneExists = await _context.Users.AnyAsync(u => u.Phone == dto.Phone && u.Id != id);
            if (phoneExists)
            {
                return (false, "Số điện thoại đã được sử dụng", null);
            }
        }

        // Update fields
        user.FullName = dto.FullName;
        user.Phone = dto.Phone;

        if (dto.Role.HasValue)
        {
            if (dto.Role.Value != UserRole.Staff && dto.Role.Value != UserRole.Admin)
            {
                return (false, "Vai trò không hợp lệ", null);
            }
            user.Role = dto.Role.Value;
        }

        if (dto.IsActive.HasValue)
        {
            user.IsActive = dto.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated staff account: {UserId}", id);

        var staffDto = new StaffDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return (true, "Cập nhật thông tin nhân viên thành công", staffDto);
    }

    // CN071: Delete (deactivate) staff
    public async Task<(bool Success, string Message)> DeleteStaffAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return (false, "Nhân viên không tồn tại");
        }

        if (user.Role != UserRole.Staff && user.Role != UserRole.Admin)
        {
            return (false, "Không thể xóa tài khoản này");
        }

        // Soft delete
        user.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated staff account: {UserId}", id);

        return (true, "Xóa tài khoản nhân viên thành công");
    }

    // ============================================
    // SHIFT MANAGEMENT
    // ============================================

    // CN075: Get all shifts với filter
    public async Task<(List<ShiftDto> Shifts, int TotalCount)> GetAllShiftsAsync(ShiftFilterDto filter)
    {
        var query = _context.StaffShifts
            .Include(s => s.Staff)
            .AsQueryable();

        // Apply filters
        if (filter.StaffId.HasValue)
        {
            query = query.Where(s => s.StaffId == filter.StaffId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(s => s.ShiftDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(s => s.ShiftDate <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var shifts = await query
            .OrderBy(s => s.ShiftDate)
            .ThenBy(s => s.StartTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new ShiftDto
            {
                Id = s.Id,
                StaffId = s.StaffId,
                StaffName = s.Staff.FullName,
                StaffEmail = s.Staff.Email,
                ShiftDate = s.ShiftDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Duration = FormatDuration(s.StartTime, s.EndTime),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return (shifts, totalCount);
    }

    public async Task<ShiftDto?> GetShiftByIdAsync(int id)
    {
        var shift = await _context.StaffShifts
            .Include(s => s.Staff)
            .Where(s => s.Id == id)
            .Select(s => new ShiftDto
            {
                Id = s.Id,
                StaffId = s.StaffId,
                StaffName = s.Staff.FullName,
                StaffEmail = s.Staff.Email,
                ShiftDate = s.ShiftDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Duration = FormatDuration(s.StartTime, s.EndTime),
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        return shift;
    }

    // CN042: Get staff's shifts
    public async Task<List<ShiftDto>> GetStaffShiftsAsync(int staffId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.StaffShifts
            .Include(s => s.Staff)
            .Where(s => s.StaffId == staffId);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.ShiftDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.ShiftDate <= toDate.Value);
        }

        var rawShifts = await query
            .OrderBy(s => s.ShiftDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.StaffId,
                StaffName = s.Staff.FullName,
                StaffEmail = s.Staff.Email,
                s.ShiftDate,
                s.StartTime,
                s.EndTime,
                s.CreatedAt
            })
            .ToListAsync();

        var shifts = rawShifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            StaffId = s.StaffId,
            StaffName = s.StaffName,
            StaffEmail = s.StaffEmail,
            ShiftDate = s.ShiftDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Duration = FormatDuration(s.StartTime, s.EndTime), 
            CreatedAt = s.CreatedAt
        }).ToList();

        return shifts;
    }

    // CN075: Get shift calendar view
    public async Task<List<ShiftCalendarDto>> GetShiftCalendarAsync(DateTime fromDate, DateTime toDate)
    {
        var shifts = await _context.StaffShifts
            .Include(s => s.Staff)
            .Where(s => s.ShiftDate >= fromDate && s.ShiftDate <= toDate)
            .ToListAsync();

        var calendar = shifts
            .GroupBy(s => s.ShiftDate.Date)
            .Select(g => new ShiftCalendarDto
            {
                Date = g.Key,
                Shifts = g.Select(s => new ShiftDto
                {
                    Id = s.Id,
                    StaffId = s.StaffId,
                    StaffName = s.Staff.FullName,
                    StaffEmail = s.Staff.Email,
                    ShiftDate = s.ShiftDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Duration = FormatDuration(s.StartTime, s.EndTime),
                    CreatedAt = s.CreatedAt
                }).ToList(),
                TotalStaff = g.Select(s => s.StaffId).Distinct().Count()
            })
            .OrderBy(c => c.Date)
            .ToList();

        return calendar;
    }

    // CN074: Create shift
    public async Task<(bool Success, string Message, ShiftDto? Data)> CreateShiftAsync(CreateShiftDto dto)
    {
        // Validate staff exists
        var staff = await _context.Users.FindAsync(dto.StaffId);
        if (staff == null || (staff.Role != UserRole.Staff && staff.Role != UserRole.Admin))
        {
            return (false, "Nhân viên không tồn tại", null);
        }

        // Validate times
        if (dto.EndTime <= dto.StartTime)
        {
            return (false, "Giờ kết thúc phải sau giờ bắt đầu", null);
        }

        // Check for conflicts
        var isAvailable = await IsStaffAvailableAsync(dto.StaffId, dto.ShiftDate, dto.StartTime, dto.EndTime);
        if (!isAvailable)
        {
            return (false, "Nhân viên đã có ca làm trùng giờ", null);
        }

        var shift = new StaffShift
        {
            StaffId = dto.StaffId,
            ShiftDate = dto.ShiftDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        _context.StaffShifts.Add(shift);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created shift for staff {StaffId} on {Date}", dto.StaffId, dto.ShiftDate);

        var shiftDto = new ShiftDto
        {
            Id = shift.Id,
            StaffId = shift.StaffId,
            StaffName = staff.FullName,
            StaffEmail = staff.Email,
            ShiftDate = shift.ShiftDate,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            Duration = FormatDuration(shift.StartTime, shift.EndTime),
            CreatedAt = shift.CreatedAt
        };

        return (true, "Tạo ca làm việc thành công", shiftDto);
    }

    // Update shift
    public async Task<(bool Success, string Message, ShiftDto? Data)> UpdateShiftAsync(int id, UpdateShiftDto dto)
    {
        var shift = await _context.StaffShifts
            .Include(s => s.Staff)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shift == null)
        {
            return (false, "Ca làm việc không tồn tại", null);
        }

        // Validate times
        if (dto.EndTime <= dto.StartTime)
        {
            return (false, "Giờ kết thúc phải sau giờ bắt đầu", null);
        }

        // Check for conflicts (excluding current shift)
        var isAvailable = await IsStaffAvailableAsync(shift.StaffId, dto.ShiftDate, dto.StartTime, dto.EndTime, id);
        if (!isAvailable)
        {
            return (false, "Nhân viên đã có ca làm trùng giờ", null);
        }

        shift.ShiftDate = dto.ShiftDate;
        shift.StartTime = dto.StartTime;
        shift.EndTime = dto.EndTime;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated shift {ShiftId}", id);

        var shiftDto = new ShiftDto
        {
            Id = shift.Id,
            StaffId = shift.StaffId,
            StaffName = shift.Staff.FullName,
            StaffEmail = shift.Staff.Email,
            ShiftDate = shift.ShiftDate,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            Duration = FormatDuration(shift.StartTime, shift.EndTime),
            CreatedAt = shift.CreatedAt
        };

        return (true, "Cập nhật ca làm việc thành công", shiftDto);
    }

    // Delete shift
    public async Task<(bool Success, string Message)> DeleteShiftAsync(int id)
    {
        var shift = await _context.StaffShifts.FindAsync(id);

        if (shift == null)
        {
            return (false, "Ca làm việc không tồn tại");
        }

        _context.StaffShifts.Remove(shift);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted shift {ShiftId}", id);

        return (true, "Xóa ca làm việc thành công");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    public async Task<bool> IsStaffAvailableAsync(int staffId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null)
    {
        var conflictingShifts = await _context.StaffShifts
            .Where(s => s.StaffId == staffId && s.ShiftDate.Date == shiftDate.Date)
            .Where(s => excludeShiftId == null || s.Id != excludeShiftId.Value)
            .Where(s =>
                (startTime >= s.StartTime && startTime < s.EndTime) ||
                (endTime > s.StartTime && endTime <= s.EndTime) ||
                (startTime <= s.StartTime && endTime >= s.EndTime)
            )
            .AnyAsync();

        return !conflictingShifts;
    }

    private static string FormatDuration(TimeSpan start, TimeSpan end)
    {
        var duration = end - start;
        return $"{duration.TotalHours:F1} giờ";
    }
}