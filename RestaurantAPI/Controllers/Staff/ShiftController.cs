using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Shift;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Staff;

[ApiController]
[Route("api/staff/my-shifts")]
[Authorize(Roles = "Staff,Admin")]

public class StaffShiftController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<StaffShiftController> _logger;

    public StaffShiftController(IStaffService staffService, ILogger<StaffShiftController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    // CN042: Xem ca làm việc của mình
    /// <summary>
    /// Xem ca làm việc của nhân viên hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyShifts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var shifts = await _staffService.GetStaffShiftsAsync(
                staffId,
                fromDate ?? DateTime.UtcNow.Date,
                toDate ?? DateTime.UtcNow.Date.AddMonths(1)
            );

            return Ok(ApiResponse<List<ShiftDto>>.SuccessResponse(
                shifts,
                "Lấy lịch làm việc thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my shifts");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }
}