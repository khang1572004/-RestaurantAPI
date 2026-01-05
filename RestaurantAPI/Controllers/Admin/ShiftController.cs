using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Shift;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/shifts")]
[Authorize(Roles = "Admin")]
public class AdminShiftController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<AdminShiftController> _logger;

    public AdminShiftController(IStaffService staffService, ILogger<AdminShiftController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    // CN075: Xem danh sách shifts
    /// <summary>
    /// Lấy danh sách ca làm việc với filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllShifts([FromQuery] ShiftFilterDto filter)
    {
        try
        {
            var (shifts, totalCount) = await _staffService.GetAllShiftsAsync(filter);

            var response = new
            {
                Items = shifts,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                response,
                "Lấy danh sách ca làm việc thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shifts");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Xem calendar view
    /// <summary>
    /// Xem lịch phân ca theo khoảng thời gian
    /// </summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> GetShiftCalendar([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var calendar = await _staffService.GetShiftCalendarAsync(fromDate, toDate);

            return Ok(ApiResponse<List<ShiftCalendarDto>>.SuccessResponse(
                calendar,
                "Lấy lịch phân ca thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shift calendar");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Xem chi tiết shift
    /// <summary>
    /// Xem chi tiết ca làm việc
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShiftById(int id)
    {
        try
        {
            var shift = await _staffService.GetShiftByIdAsync(id);

            if (shift == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Ca làm việc không tồn tại"));

            return Ok(ApiResponse<ShiftDto>.SuccessResponse(shift));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shift {ShiftId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN074: Tạo shift
    /// <summary>
    /// Phân ca làm việc cho nhân viên
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
    {
        try
        {
            var (success, message, data) = await _staffService.CreateShiftAsync(dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<ShiftDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi tạo ca làm việc"));
        }
    }

    // Update shift
    /// <summary>
    /// Cập nhật ca làm việc
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftDto dto)
    {
        try
        {
            var (success, message, data) = await _staffService.UpdateShiftAsync(id, dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<ShiftDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift {ShiftId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi cập nhật"));
        }
    }

    // Delete shift
    /// <summary>
    /// Xóa ca làm việc
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShift(int id)
    {
        try
        {
            var (success, message) = await _staffService.DeleteShiftAsync(id);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<bool>.SuccessResponse(true, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift {ShiftId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi xóa"));
        }
    }
}