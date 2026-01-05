using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Staff;

[ApiController]
[Route("api/staff/bookings")]
[Authorize(Roles = "Staff,Admin")]
public class StaffBookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<StaffBookingController> _logger;

    public StaffBookingController(
        IBookingService bookingService,
        ILogger<StaffBookingController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// CN029: Xem danh sách đặt bàn hôm nay
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayBookings()
    {
        var bookings = await _bookingService.GetBookingsByDateAsync(DateTime.UtcNow.Date);
        return Ok(ApiResponse<List<BookingDto>>.SuccessResponse(bookings));
    }

    /// <summary>
    /// CN029: Xem danh sách đặt bàn theo ngày
    /// </summary>
    [HttpGet("date")]
    public async Task<IActionResult> GetBookingsByDate([FromQuery] DateTime date)
    {
        var bookings = await _bookingService.GetBookingsByDateAsync(date.Date);
        return Ok(ApiResponse<List<BookingDto>>.SuccessResponse(bookings));
    }

    /// <summary>
    /// CN030: Xem sơ đồ bàn và trạng thái
    /// </summary>
    [HttpGet("../tables/status")]
    public async Task<IActionResult> GetTableStatus()
    {
        var tables = await _bookingService.GetTableStatusAsync();
        return Ok(ApiResponse<List<TableStatusDto>>.SuccessResponse(tables));
    }

    /// <summary>
    /// CN031: Check-in khách khi đến
    /// </summary>
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message, data) = await _bookingService.CheckInBookingAsync(dto.BookingCode);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN033: Đánh dấu No-show
    /// </summary>
    [HttpPost("{id}/no-show")]
    public async Task<IActionResult> MarkAsNoShow(int id)
    {
        var (success, message) = await _bookingService.MarkAsNoShowAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN034: Tạo đặt bàn trực tiếp (Walk-in)
    /// </summary>
    [HttpPost("walk-in")]
    public async Task<IActionResult> CreateWalkIn([FromBody] CreateWalkInDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var staffId = GetUserId();
        var (success, message, data) = await _bookingService.CreateWalkInBookingAsync(dto, staffId);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN036: Check-out và giải phóng bàn
    /// </summary>
    [HttpPost("{id}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var (success, message) = await _bookingService.CheckOutBookingAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// Xem chi tiết booking (Staff)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy booking"));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking));
    }
}