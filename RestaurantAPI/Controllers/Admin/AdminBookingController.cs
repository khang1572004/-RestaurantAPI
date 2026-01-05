using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin")]
public class AdminBookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<AdminBookingController> _logger;

    public AdminBookingController(
        IBookingService bookingService,
        ILogger<AdminBookingController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// CN065: Xem tất cả đặt bàn với filter, search, pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BookingFilterDto filter)
    {
        var (bookings, totalCount) = await _bookingService.GetAllBookingsAsync(filter);

        var response = new
        {
            Items = bookings,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };

        return Ok(ApiResponse<object>.SuccessResponse(response));
    }

    /// <summary>
    /// CN066: Xem chi tiết đặt bàn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy booking"));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking));
    }

    /// <summary>
    /// CN067: Duyệt đặt bàn
    /// </summary>
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var (success, message) = await _bookingService.ApproveBookingAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN067: Từ chối đặt bàn
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] CancelBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message) = await _bookingService.RejectBookingAsync(id, dto.Reason);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN068: Hủy đặt bàn (Admin)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _bookingService.DeleteBookingAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN069: Sửa đổi đặt bàn
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _bookingService.UpdateBookingAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(data!, message));
    }
}