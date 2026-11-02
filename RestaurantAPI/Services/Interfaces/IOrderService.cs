using RestaurantAPI.DTOs.Order;

namespace RestaurantAPI.Services.Interfaces;

public interface IOrderService
{
    // CN037: Tạo order cho bàn
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int staffId);

    // CN038: Thêm món vào order
    Task<OrderItemDto> AddOrderItemAsync(int orderId, AddOrderItemDto dto);

    // CN038: Cập nhật số lượng món trong order
    Task<OrderItemDto> UpdateOrderItemAsync(int orderId, int orderItemId, UpdateOrderItemDto dto);

    // CN038: Xóa món khỏi order
    Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId);

    // CN039: Áp dụng voucher vào order
    Task<OrderDto> ApplyVoucherAsync(int orderId, ApplyVoucherDto dto);

    // CN040: Xem tổng hóa đơn
    Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId);

    // CN041: Thanh toán và in hóa đơn
    Task<OrderPaymentResponseDto> ProcessPaymentAsync(int orderId, ProcessOrderPaymentDto dto, int staffId);

    // Helper methods
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<List<OrderDto>> GetOrdersByBookingIdAsync(int bookingId);
    Task<List<OrderDto>> GetOrdersByTableIdAsync(int tableId);
    Task<List<OrderDto>> GetOrderHistoryAsync(int? tableId, int? bookingId);
    Task<bool> CancelOrderAsync(int orderId, int staffId);
}
