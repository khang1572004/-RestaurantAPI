using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Order;

// DTO để tạo order mới
public class CreateOrderDto
{
    public int? BookingId { get; set; }
    
    public int? TableId { get; set; }

    // Validate: Phải có ít nhất 1 trong 2 (BookingId hoặc TableId)
    public void Validate()
    {
        if (!BookingId.HasValue && !TableId.HasValue)
        {
            throw new ArgumentException("Phải cung cấp BookingId hoặc TableId");
        }
    }
}

// DTO để thêm món vào order
public class AddOrderItemDto
{
    [Required(ErrorMessage = "Menu Item ID là bắt buộc")]
    public int MenuItemId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1-100")]
    public int Quantity { get; set; }
}

// DTO để update số lượng món
public class UpdateOrderItemDto
{
    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1-100")]
    public int Quantity { get; set; }
}

// DTO để apply voucher vào order
public class ApplyVoucherDto
{
    [Required(ErrorMessage = "Mã voucher là bắt buộc")]
    [MaxLength(50)]
    public string VoucherCode { get; set; } = string.Empty;
}

// DTO để thanh toán order
public class ProcessOrderPaymentDto
{
    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    public PaymentMethod PaymentMethod { get; set; }
}

// DTO response cho order
public class OrderDto
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public string? BookingCode { get; set; }
    public int TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public int? VoucherId { get; set; }
    public string? VoucherCode { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

// DTO cho order item
public class OrderItemDto
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string? MenuItemImage { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

// DTO cho order summary (tổng hóa đơn)
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? VoucherCode { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal TotalAmount { get; set; } // Subtotal - Discount
    public decimal FinalAmount { get; set; } // TotalAmount - Deposit (số tiền cần thanh toán)
    public List<OrderItemDto> Items { get; set; } = new();
    
    public string Summary => $@"
=====================================
           HÓA ĐỚN THANH TOÁN
=====================================
Tổng tiền món ăn:    {Subtotal:N0} VNĐ
Giảm giá ({VoucherCode}): -{DiscountAmount:N0} VNĐ
-------------------------------------
Tổng cộng:           {TotalAmount:N0} VNĐ
Đã đặt cọc:         -{DepositAmount:N0} VNĐ
=====================================
CÒN PHẢI TRẢ:        {FinalAmount:N0} VNĐ
=====================================
    ";
}

// DTO cho payment response
public class OrderPaymentResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? PaymentId { get; set; }
    public string? InvoiceUrl { get; set; }
    public OrderSummaryDto? OrderSummary { get; set; }
}