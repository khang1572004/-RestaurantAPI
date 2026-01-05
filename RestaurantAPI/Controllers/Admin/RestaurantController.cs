using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantController> _logger;

    public RestaurantController(IRestaurantService restaurantService, ILogger<RestaurantController> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    /// <summary>
    /// CN096: Xem cài đặt nhà hàng
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var settings = await _restaurantService.GetRestaurantInfoAsync();
            return Ok(ApiResponse<RestaurantInfoDto>.SuccessResponse(settings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant settings");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi khi lấy cài đặt"));
        }
    }

    /// <summary>
    /// CN092: Cập nhật thông tin nhà hàng
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateRestaurantSettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var success = await _restaurantService.UpdateRestaurantSettingsAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cập nhật thông tin thất bại"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật thông tin nhà hàng thành công"));
    }

    /// <summary>
    /// CN093: Cập nhật giờ mở cửa
    /// </summary>
    [HttpPut("opening-hours")]
    public async Task<IActionResult> UpdateOpeningHours([FromBody] UpdateOpeningHoursDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var success = await _restaurantService.UpdateOpeningHoursAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cập nhật giờ mở cửa thất bại"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật giờ mở cửa thành công"));
    }

    /// <summary>
    /// CN094: Cập nhật chính sách đặt bàn
    /// </summary>
    [HttpPut("booking-policy")]
    public async Task<IActionResult> UpdateBookingPolicy([FromBody] UpdateBookingPolicyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var success = await _restaurantService.UpdateBookingPolicyAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cập nhật chính sách thất bại"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật chính sách đặt bàn thành công"));
    }
}