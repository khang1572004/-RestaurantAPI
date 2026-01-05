using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.User;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    // CN073: Xem danh sách staff
    /// <summary>
    /// Lấy danh sách nhân viên với filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllStaff([FromQuery] StaffFilterDto filter)
    {
        try
        {
            var (staff, totalCount) = await _staffService.GetAllStaffAsync(filter);

            var response = new
            {
                Items = staff,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                response,
                "Lấy danh sách nhân viên thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff list");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Xem chi tiết staff
    /// <summary>
    /// Xem chi tiết nhân viên
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStaffById(int id)
    {
        try
        {
            var staff = await _staffService.GetStaffByIdAsync(id);

            if (staff == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Nhân viên không tồn tại"));

            return Ok(ApiResponse<StaffDto>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff {StaffId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN070: Tạo staff mới
    /// <summary>
    /// Tạo tài khoản nhân viên mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        try
        {
            var (success, message, data) = await _staffService.CreateStaffAsync(dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<StaffDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi tạo nhân viên"));
        }
    }

    // CN072: Update staff
    /// <summary>
    /// Cập nhật thông tin nhân viên
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        try
        {
            var (success, message, data) = await _staffService.UpdateStaffAsync(id, dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<StaffDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff {StaffId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi cập nhật"));
        }
    }

    // CN071: Delete staff
    /// <summary>
    /// Xóa (vô hiệu hóa) tài khoản nhân viên
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        try
        {
            var (success, message) = await _staffService.DeleteStaffAsync(id);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<bool>.SuccessResponse(true, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff {StaffId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi xóa"));
        }
    }
}