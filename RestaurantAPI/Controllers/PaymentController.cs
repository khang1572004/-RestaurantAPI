using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Payment;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;

    public PaymentController(
        IPaymentService paymentService, 
        ILogger<PaymentController> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// CN017: Tạo thanh toán cọc
    /// </summary>
    [HttpPost("create-deposit")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositPaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        // ✅ Get authenticated user ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Không thể xác thực người dùng"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        
        _logger.LogInformation(
            "User {UserId} creating payment for booking {BookingId} from IP {IP}",
            userId, dto.BookingId, ipAddress);

        var result = await _paymentService.CreateDepositPaymentAsync(dto, ipAddress);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<CreatePaymentResponse>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<CreatePaymentResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// CN018: VNPay callback
    /// </summary>
    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VNPayCallback()
    {
        try
        {
            _logger.LogInformation("VNPay callback received from IP: {IP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var (success, message, payment) = await _paymentService.ProcessVNPayCallbackAsync(Request.Query);

            var frontendUrl = GetFrontendUrl();

            if (!success)
            {
                _logger.LogWarning("VNPay callback failed: {Message}", message);
                return Redirect($"{frontendUrl}/booking/payment-result?success=false&message={Uri.EscapeDataString(message)}");
            }

            _logger.LogInformation(
                "VNPay callback successful for booking {BookingCode}", 
                payment?.BookingCode);

            return Redirect($"{frontendUrl}/booking/payment-result?success=true&bookingCode={payment?.BookingCode}&amount={payment?.Amount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay callback");
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/booking/payment-result?success=false&message={Uri.EscapeDataString("Đã xảy ra lỗi")}");
        }
    }

    /// <summary>
    /// CN018: MoMo callback
    /// </summary>
    [HttpGet("momo-callback")]
    public async Task<IActionResult> MoMoCallback()
    {
        try
        {
            _logger.LogInformation("MoMo callback received from IP: {IP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var (success, message, payment) = await _paymentService.ProcessMoMoCallbackAsync(Request.Query);

            var frontendUrl = GetFrontendUrl();

            if (!success)
            {
                _logger.LogWarning("MoMo callback failed: {Message}", message);
                return Redirect($"{frontendUrl}/booking/payment-result?success=false&message={Uri.EscapeDataString(message)}");
            }

            _logger.LogInformation(
                "MoMo callback successful for booking {BookingCode}", 
                payment?.BookingCode);

            return Redirect($"{frontendUrl}/booking/payment-result?success=true&bookingCode={payment?.BookingCode}&amount={payment?.Amount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo callback");
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/booking/payment-result?success=false&message={Uri.EscapeDataString("Đã xảy ra lỗi")}");
        }
    }

    /// <summary>
    /// MoMo IPN (Instant Payment Notification)
    /// </summary>
    [HttpPost("momo-ipn")]
    public async Task<IActionResult> MoMoIPN()
    {
        try
        {
            _logger.LogInformation("MoMo IPN received from IP: {IP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var (success, message, payment) = await _paymentService.ProcessMoMoCallbackAsync(Request.Query);

            if (success)
            {
                _logger.LogInformation("MoMo IPN processed successfully");
                return Ok(new { resultCode = 0, message = "Success" });
            }

            _logger.LogWarning("MoMo IPN failed: {Message}", message);
            return Ok(new { resultCode = 1, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return Ok(new { resultCode = 1, message = "Error" });
        }
    }

    /// <summary>
    /// Xem lịch sử thanh toán của khách hàng
    /// </summary>
    [HttpGet("my-payments")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPayments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Không thể xác thực người dùng"));
        }

        var payments = await _paymentService.GetPaymentsByCustomerIdAsync(userId);
        return Ok(ApiResponse<List<PaymentDto>>.SuccessResponse(payments));
    }

    /// <summary>
    /// Xem thanh toán theo booking (Admin/Staff có thể xem tất cả, Customer chỉ xem của mình)
    /// </summary>
    [HttpGet("booking/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentsByBooking(int bookingId)
    {
        // TODO: Add authorization check - customers should only see their own bookings
        var payments = await _paymentService.GetPaymentsByBookingIdAsync(bookingId);
        return Ok(ApiResponse<List<PaymentDto>>.SuccessResponse(payments));
    }

    /// <summary>
    /// Xem chi tiết một payment (Admin/Staff only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        
        if (payment == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy payment"));
        }

        return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment));
    }

    /// <summary>
    /// ✅ Get frontend URL from configuration
    /// </summary>
    private string GetFrontendUrl()
    {
        return _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
    }
}