using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Payment;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class RefundController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IVNPayService _vnpayService;
    private readonly IMoMoService _momoService;
    private readonly ILogger<RefundController> _logger;

    public RefundController(
        ApplicationDbContext context,
        IVNPayService vnpayService,
        IMoMoService momoService,
        ILogger<RefundController> logger)
    {
        _context = context;
        _vnpayService = vnpayService;
        _momoService = momoService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// CN084: Xem danh sách yêu cầu hoàn tiền
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RefundStatus? status = null)
    {
        var query = _context.RefundRequests
            .Include(r => r.Booking).ThenInclude(b => b.Customer)
            .Include(r => r.Payment)
            .Include(r => r.ProcessedByUser)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var refunds = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new RefundRequestDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                BookingCode = r.Booking.BookingCode,
                CustomerName = r.Booking.Customer.FullName,
                PaymentId = r.PaymentId,
                RefundAmount = r.RefundAmount,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ProcessedAt = r.ProcessedAt,
                ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null,
                AdminNote = r.AdminNote
            })
            .ToListAsync();

        return Ok(ApiResponse<List<RefundRequestDto>>.SuccessResponse(refunds));
    }

    /// <summary>
    /// Xem chi tiết yêu cầu hoàn tiền
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var refund = await _context.RefundRequests
            .Include(r => r.Booking).ThenInclude(b => b.Customer)
            .Include(r => r.Payment)
            .Include(r => r.ProcessedByUser)
            .Where(r => r.Id == id)
            .Select(r => new RefundRequestDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                BookingCode = r.Booking.BookingCode,
                CustomerName = r.Booking.Customer.FullName,
                PaymentId = r.PaymentId,
                RefundAmount = r.RefundAmount,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ProcessedAt = r.ProcessedAt,
                ProcessedByName = r.ProcessedByUser != null ? r.ProcessedByUser.FullName : null,
                AdminNote = r.AdminNote
            })
            .FirstOrDefaultAsync();

        if (refund == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy yêu cầu hoàn tiền"));
        }

        return Ok(ApiResponse<RefundRequestDto>.SuccessResponse(refund));
    }

    /// <summary>
    /// CN084: Duyệt yêu cầu hoàn tiền
    /// </summary>
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ProcessRefundDto dto)
    {
        try
        {
            var refund = await _context.RefundRequests
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy yêu cầu hoàn tiền"));
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ có thể duyệt yêu cầu đang chờ xử lý"));
            }

            refund.Status = RefundStatus.Approved;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.ProcessedBy = GetUserId();
            refund.AdminNote = dto.AdminNote;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund request {RefundId} approved by admin {AdminId}", id, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(null, "Duyệt yêu cầu hoàn tiền thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving refund {RefundId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi"));
        }
    }

    /// <summary>
    /// CN084: Từ chối yêu cầu hoàn tiền
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ProcessRefundDto dto)
    {
        try
        {
            var refund = await _context.RefundRequests.FindAsync(id);

            if (refund == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy yêu cầu hoàn tiền"));
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ có thể từ chối yêu cầu đang chờ xử lý"));
            }

            refund.Status = RefundStatus.Rejected;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.ProcessedBy = GetUserId();
            refund.AdminNote = dto.AdminNote;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund request {RefundId} rejected by admin {AdminId}", id, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(null, "Từ chối yêu cầu hoàn tiền"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting refund {RefundId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi"));
        }
    }

    /// <summary>
    /// CN084: Xử lý hoàn tiền (gọi API gateway)
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(int id)
    {
        try
        {
            var refund = await _context.RefundRequests
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy yêu cầu hoàn tiền"));
            }

            if (refund.Status != RefundStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ có thể xử lý yêu cầu đã được duyệt"));
            }

            var payment = refund.Payment;
            if (payment == null || string.IsNullOrEmpty(payment.TransactionId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Không tìm thấy thông tin thanh toán"));
            }

            bool success = false;
            string message = "";

            // Process refund based on payment method
            if (payment.PaymentMethod == PaymentMethod.VNPay)
            {
                (success, message) = await _vnpayService.ProcessRefundAsync(
                    payment.TransactionId,
                    refund.RefundAmount,
                    refund.Reason ?? "Customer cancellation"
                );
            }
            else if (payment.PaymentMethod == PaymentMethod.MoMo)
            {
                (success, message) = await _momoService.ProcessRefundAsync(
                    payment.TransactionId,
                    refund.RefundAmount,
                    refund.Reason ?? "Customer cancellation"
                );
            }
            else
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Phương thức thanh toán này chưa hỗ trợ hoàn tiền tự động"));
            }

            if (success)
            {
                refund.Status = RefundStatus.Completed;
                payment.Status = PaymentStatusEnum.Refunded;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refund {RefundId} processed successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Hoàn tiền thành công"));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund {RefundId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi khi xử lý hoàn tiền"));
        }
    }
}
