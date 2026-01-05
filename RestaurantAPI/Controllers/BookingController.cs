using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.DTOs.Voucher;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ITableAvailabilityService _availabilityService;
    private readonly IVoucherService _voucherService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingService bookingService,
        ITableAvailabilityService availabilityService,
        IVoucherService voucherService,
        ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _availabilityService = availabilityService;
        _voucherService = voucherService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// CN012: Xem danh sách bàn còn trống
    /// </summary>
    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability([FromQuery] CheckAvailabilityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var tables = await _availabilityService.GetAvailableTablesAsync(
            dto.BookingDate, dto.BookingTime, dto.NumGuests);

        return Ok(ApiResponse<List<AvailableTableDto>>.SuccessResponse(tables));
    }

    /// <summary>
    /// CN010-CN019: Đặt bàn
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var userId = GetUserId();
        var (success, message, data) = await _bookingService.CreateBookingAsync(dto, userId);

        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN020: Xem danh sách đặt bàn của tôi
    /// </summary>
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings([FromQuery] string? filter = null)
    {
        var userId = GetUserId();
        var bookings = await _bookingService.GetCustomerBookingsAsync(userId, filter);

        return Ok(ApiResponse<List<BookingDto>>.SuccessResponse(bookings));
    }

    /// <summary>
    /// CN021: Xem chi tiết đặt bàn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.GetBookingByIdAsync(id, userId);

        if (booking == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy booking"));
        }

        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking));
    }

    /// <summary>
    /// CN022: Hủy đặt bàn
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var userId = GetUserId();
        var (success, message) = await _bookingService.CancelBookingAsync(id, userId, dto.Reason);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN023: Xem lịch sử giao dịch thanh toán
    /// (Sẽ implement đầy đủ ở Phase 4 - Payment)
    /// </summary>
    [HttpGet("my-payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        // TODO: Implement in Phase 4
        return Ok(ApiResponse<List<object>>.SuccessResponse(new List<object>(), 
            "Chức năng thanh toán sẽ được triển khai ở Phase 4"));
    }
}

// ==================== VOUCHER CONTROLLER (PUBLIC) ====================
[ApiController]
[Route("api/[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VoucherController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>
    /// CN009: Xem khuyến mãi đang có (Public)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveVouchers()
    {
        var vouchers = await _voucherService.GetActiveVouchersAsync();
        return Ok(ApiResponse<List<VoucherDto>>.SuccessResponse(vouchers));
    }

    /// <summary>
    /// Validate voucher (Customer)
    /// </summary>
    [HttpPost("validate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.Parse(userIdClaim ?? "0");

        var result = await _voucherService.ValidateVoucherAsync(dto.Code, userId, dto.OrderValue);

        if (!result.IsValid)
        {
            return BadRequest(ApiResponse<VoucherValidationResult>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<VoucherValidationResult>.SuccessResponse(result));
    }
}