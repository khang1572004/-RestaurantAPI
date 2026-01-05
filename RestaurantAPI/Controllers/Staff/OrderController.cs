using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Order;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Staff;

[ApiController]
[Route("api/staff/orders")]
[Authorize(Roles = "Staff,Admin")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    // CN037: Tạo order cho bàn
    /// <summary>
    /// Tạo order mới cho bàn (walk-in hoặc từ booking)
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(dto, staffId);

            return Ok(ApiResponse<OrderDto>.SuccessResponse(
                order,
                "Tạo order thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi tạo order"));
        }
    }

    // CN038: Thêm món vào order
    /// <summary>
    /// Thêm món ăn vào order
    /// </summary>
    [HttpPost("{orderId}/items")]
    public async Task<IActionResult> AddOrderItem(int orderId, [FromBody] AddOrderItemDto dto)
    {
        try
        {
            var orderItem = await _orderService.AddOrderItemAsync(orderId, dto);

            return Ok(ApiResponse<OrderItemDto>.SuccessResponse(
                orderItem,
                "Thêm món thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding order item");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi thêm món"));
        }
    }

    // CN038: Cập nhật số lượng món
    /// <summary>
    /// Cập nhật số lượng món trong order
    /// </summary>
    [HttpPut("{orderId}/items/{orderItemId}")]
    public async Task<IActionResult> UpdateOrderItem(int orderId, int orderItemId, [FromBody] UpdateOrderItemDto dto)
    {
        try
        {
            var orderItem = await _orderService.UpdateOrderItemAsync(orderId, orderItemId, dto);

            return Ok(ApiResponse<OrderItemDto>.SuccessResponse(
                orderItem,
                "Cập nhật món thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order item");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi cập nhật món"));
        }
    }

    // CN038: Xóa món khỏi order
    /// <summary>
    /// Xóa món khỏi order
    /// </summary>
    [HttpDelete("{orderId}/items/{orderItemId}")]
    public async Task<IActionResult> RemoveOrderItem(int orderId, int orderItemId)
    {
        try
        {
            var result = await _orderService.RemoveOrderItemAsync(orderId, orderItemId);

            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa món thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing order item");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi xóa món"));
        }
    }

    // CN039: Áp dụng voucher
    /// <summary>
    /// Áp dụng voucher vào order
    /// </summary>
    [HttpPut("{orderId}/apply-voucher")]
    public async Task<IActionResult> ApplyVoucher(int orderId, [FromBody] ApplyVoucherDto dto)
    {
        try
        {
            var order = await _orderService.ApplyVoucherAsync(orderId, dto);

            return Ok(ApiResponse<OrderDto>.SuccessResponse(
                order,
                "Áp dụng voucher thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying voucher");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi áp dụng voucher"));
        }
    }

    // CN040: Xem tổng hóa đơn
    /// <summary>
    /// Xem tổng hóa đơn trước khi thanh toán
    /// </summary>
    [HttpGet("{orderId}/summary")]
    public async Task<IActionResult> GetOrderSummary(int orderId)
    {
        try
        {
            var summary = await _orderService.GetOrderSummaryAsync(orderId);

            return Ok(ApiResponse<OrderSummaryDto>.SuccessResponse(
                summary,
                "Lấy thông tin hóa đơn thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN041: Thanh toán và in hóa đơn
    /// <summary>
    /// Thanh toán order (tự động check-out booking và giải phóng bàn)
    /// </summary>
    [HttpPost("{orderId}/pay")]
    public async Task<IActionResult> ProcessPayment(int orderId, [FromBody] ProcessOrderPaymentDto dto)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var result = await _orderService.ProcessPaymentAsync(orderId, dto, staffId);

            return Ok(ApiResponse<OrderPaymentResponseDto>.SuccessResponse(
                result,
                "Thanh toán thành công"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi thanh toán"));
        }
    }

    // Bonus: Xem chi tiết order
    /// <summary>
    /// Xem chi tiết order
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Order không tồn tại"));

            return Ok(ApiResponse<OrderDto>.SuccessResponse(
                order,
                "Lấy thông tin order thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Bonus: Hủy order
    /// <summary>
    /// Hủy order (chỉ khi chưa thanh toán)
    /// </summary>
    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var result = await _orderService.CancelOrderAsync(orderId, staffId);

            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Hủy order thành công"
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi hủy order"));
        }
    }
}