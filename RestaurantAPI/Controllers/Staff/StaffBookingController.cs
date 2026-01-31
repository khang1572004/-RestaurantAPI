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
    /// ✅ UPDATED: CN031 - Check-in khách khi đến (normal check-in)
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
            // ✅ CHECK IF IT'S A LATE CHECK-IN MESSAGE
            if (message.Contains("muộn") && message.Contains("late-check-in"))
            {
                return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
                    message,
                    new List<string> { "USE_LATE_CHECKIN_ENDPOINT" }
                ));
            }

            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        _logger.LogInformation(
            "Check-in successful: {BookingCode} by staff {StaffId}",
            dto.BookingCode, GetUserId());

        return Ok(ApiResponse<BookingDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// ✅ NEW: Late check-in (30-60 minutes after booking time)
    /// </summary>
    [HttpPost("late-check-in")]
    public async Task<IActionResult> LateCheckIn([FromBody] LateCheckInDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        if (!dto.AcknowledgeDepositForfeit)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Phải xác nhận rằng khách chấp nhận mất tiền cọc"
            ));
        }

        var (success, message, data) = await _bookingService.LateCheckInAsync(
            dto.BookingCode,
            dto.AcknowledgeDepositForfeit);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        _logger.LogWarning(
            "LATE check-in: {BookingCode} by staff {StaffId}, deposit forfeited",
            dto.BookingCode, GetUserId());

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

        _logger.LogInformation("Booking {BookingId} marked as no-show by staff {StaffId}", 
            id, GetUserId());

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// ✅ NEW: Check walk-in availability
    /// </summary>
    [HttpPost("walk-in/check-availability")]
    public async Task<IActionResult> CheckWalkInAvailability([FromBody] CheckWalkInAvailabilityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var result = await _bookingService.CheckWalkInAvailabilityAsync(dto.TableId, dto.WalkInTime);

        if (!result.CanWalkIn)
        {
            // Return error response with the result object
            return BadRequest(new ApiResponse<WalkInAvailabilityResponse>
            {
                Success = false,
                Message = result.Message,
                Data = result,
                Errors = null
            });
        }

        return Ok(ApiResponse<WalkInAvailabilityResponse>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// ✅ UPDATED: CN034 - Tạo đặt bàn trực tiếp (Walk-in) với service duration
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

        // ✅ VALIDATE SERVICE DURATION
        if (dto.ServiceDurationMinutes % 30 != 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Thời gian phục vụ phải là bội số của 30 phút"
            ));
        }

        var staffId = GetUserId();
        var (success, message, data) = await _bookingService.CreateWalkInBookingAsync(dto, staffId);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        _logger.LogInformation(
            "Walk-in booking created: {BookingCode} by staff {StaffId}, Duration: {Duration}m",
            data?.BookingCode, staffId, dto.ServiceDurationMinutes);

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

        _logger.LogInformation("Booking {BookingId} checked out by staff {StaffId}", 
            id, GetUserId());

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// ✅ NEW: Approve party booking (>4 hours)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApprovePartyBooking(int id, [FromBody] ApprovePartyBookingDto? dto)
    {
        var (success, message) = await _bookingService.ApprovePartyBookingAsync(id, dto?.AdminNotes);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        _logger.LogInformation(
            "Party booking {BookingId} approved by admin {AdminId}",
            id, GetUserId());

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// ✅ NEW: Reject party booking
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectPartyBooking(int id, [FromBody] RejectPartyBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message) = await _bookingService.RejectPartyBookingAsync(id, dto.Reason);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        _logger.LogInformation(
            "Party booking {BookingId} rejected by admin {AdminId}, Reason: {Reason}",
            id, GetUserId(), dto.Reason);

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