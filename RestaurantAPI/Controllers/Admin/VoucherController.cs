using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Voucher;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/vouchers")]
[Authorize(Roles = "Admin")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly ILogger<VoucherController> _logger;

    public VoucherController(IVoucherService voucherService, ILogger<VoucherController> logger)
    {
        _voucherService = voucherService;
        _logger = logger;
    }

    // CN080: Xem danh sách vouchers
    /// <summary>
    /// Lấy danh sách vouchers với filter (Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVouchers([FromQuery] VoucherFilterDto filter)
    {
        try
        {
            var (vouchers, totalCount) = await _voucherService.GetAllVouchersAsync(filter);

            var response = new
            {
                Items = vouchers,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                response,
                "Lấy danh sách vouchers thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vouchers");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Xem chi tiết voucher
    /// <summary>
    /// Xem chi tiết voucher
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVoucherById(int id)
    {
        try
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);

            if (voucher == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Voucher không tồn tại"));

            return Ok(ApiResponse<VoucherDto>.SuccessResponse(voucher));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher {VoucherId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN076: Tạo voucher
    /// <summary>
    /// Tạo voucher mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
    {
        try
        {
            var (success, message, data) = await _voucherService.CreateVoucherAsync(dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<VoucherDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi tạo voucher"));
        }
    }

    // CN077: Update voucher
    /// <summary>
    /// Cập nhật voucher
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherDto dto)
    {
        try
        {
            var (success, message, data) = await _voucherService.UpdateVoucherAsync(id, dto);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<VoucherDto>.SuccessResponse(data!, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi cập nhật"));
        }
    }

    // CN078: Delete voucher
    /// <summary>
    /// Xóa voucher (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVoucher(int id)
    {
        try
        {
            var (success, message) = await _voucherService.DeleteVoucherAsync(id);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<bool>.SuccessResponse(true, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi xóa"));
        }
    }

    // CN079: Toggle voucher
    /// <summary>
    /// Bật/tắt voucher
    /// </summary>
    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleVoucher(int id, [FromBody] ToggleVoucherDto dto)
    {
        try
        {
            var (success, message) = await _voucherService.ToggleVoucherAsync(id, dto.IsActive);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<bool>.SuccessResponse(true, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling voucher {VoucherId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN081: Xem thống kê voucher
    /// <summary>
    /// Xem thống kê sử dụng voucher
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetVoucherStats(int id)
    {
        try
        {
            var stats = await _voucherService.GetVoucherStatsAsync(id);

            return Ok(ApiResponse<VoucherStatsDto>.SuccessResponse(
                stats,
                "Lấy thống kê voucher thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher stats {VoucherId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }
}