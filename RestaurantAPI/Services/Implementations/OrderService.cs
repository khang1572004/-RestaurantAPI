using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Order;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IVoucherService _voucherService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        IVoucherService voucherService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _voucherService = voucherService;
        _logger = logger;
    }

    // CN037: Tạo order cho bàn
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int staffId)
    {
        dto.Validate();

        // Kiểm tra booking hoặc table tồn tại
        Booking? booking = null;
        Table? table = null;

        if (dto.BookingId.HasValue)
        {
            booking = await _context.Bookings
                .Include(b => b.Table)
                .Include(b => b.Voucher)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId.Value);

            if (booking == null)
                throw new ArgumentException("Booking không tồn tại");

            if (booking.Status != BookingStatus.CheckedIn)
                throw new InvalidOperationException("Booking chưa check-in");

            // Kiểm tra đã có order chưa
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.BookingId == dto.BookingId.Value && o.Status != OrderStatus.Cancelled);

            if (existingOrder != null)
                throw new InvalidOperationException("Booking này đã có order");

            table = booking.Table;
        }
        else if (dto.TableId.HasValue)
        {
            table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == dto.TableId.Value);

            if (table == null)
                throw new ArgumentException("Bàn không tồn tại");

            if (table.Status != TableStatus.Occupied)
                throw new InvalidOperationException("Bàn chưa được sử dụng");

            booking = await _context.Bookings
                .Include(b => b.Table)
                .Where(b => b.TableId == table.Id && b.Status == BookingStatus.CheckedIn)
                .OrderByDescending(b => b.CheckInTime)
                .FirstOrDefaultAsync();

            if (booking != null && await _context.Orders.AnyAsync(o => o.BookingId == booking.Id && o.Status != OrderStatus.Cancelled))
                throw new InvalidOperationException("Booking này đã có hóa đơn đang mở");
        }

        // Tạo order mới
        var order = new Order
        {
            BookingId = dto.BookingId,
            TableId = table!.Id,
            StaffId = staffId,
            Subtotal = 0,
            DiscountAmount = 0,
            VoucherId = booking?.VoucherId, // Tự động apply voucher từ booking nếu có
            DepositAmount = 0,
            FinalAmount = 0,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Nếu có booking và đã cọc, lấy deposit amount
        if (booking != null)
        {
            order.DepositAmount = await _context.Payments
                .Where(p => p.BookingId == booking.Id && p.PaymentType == PaymentType.Deposit && p.Status == PaymentStatusEnum.Success)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created order {OrderId} for table {TableId} by staff {StaffId}", order.Id, table.Id, staffId);

        return await MapToOrderDto(order);
    }

    // CN038: Thêm món vào order
    public async Task<OrderItemDto> AddOrderItemAsync(int orderId, AddOrderItemDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Không thể thêm món vào order đã thanh toán hoặc đã hủy");

        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == dto.MenuItemId);

        if (menuItem == null)
            throw new ArgumentException("Món ăn không tồn tại");

        if (!menuItem.IsAvailable)
            throw new InvalidOperationException($"Món '{menuItem.Name}' hiện không còn");

        // Kiểm tra món đã có trong order chưa
        var existingItem = order.OrderItems
            .FirstOrDefault(oi => oi.MenuItemId == dto.MenuItemId);

        if (existingItem != null)
        {
            // Cập nhật số lượng
            existingItem.Quantity += dto.Quantity;
            existingItem.Subtotal = existingItem.Price * existingItem.Quantity;
        }
        else
        {
            // Thêm mới
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                MenuItemId = dto.MenuItemId,
                Quantity = dto.Quantity,
                Price = menuItem.Price,
                Subtotal = menuItem.Price * dto.Quantity
            };

            _context.OrderItems.Add(orderItem);
            existingItem = orderItem;
        }

        // Recalculate order totals
        await RecalculateOrderTotalsAsync(order);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Added/Updated item {MenuItemId} to order {OrderId}", dto.MenuItemId, orderId);

        return await MapToOrderItemDto(existingItem);
    }

    // CN038: Cập nhật số lượng món
    public async Task<OrderItemDto> UpdateOrderItemAsync(int orderId, int orderItemId, UpdateOrderItemDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Không thể chỉnh sửa order đã thanh toán hoặc đã hủy");

        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);

        if (orderItem == null)
            throw new ArgumentException("Món ăn không có trong order");

        orderItem.Quantity = dto.Quantity;
        orderItem.Subtotal = orderItem.Price * orderItem.Quantity;

        await RecalculateOrderTotalsAsync(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated order item {OrderItemId} quantity to {Quantity}", orderItemId, dto.Quantity);

        return await MapToOrderItemDto(orderItem);
    }

    // CN038: Xóa món khỏi order
    public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Không thể xóa món khỏi order đã thanh toán hoặc đã hủy");

        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);

        if (orderItem == null)
            throw new ArgumentException("Món ăn không có trong order");

        _context.OrderItems.Remove(orderItem);
        order.OrderItems.Remove(orderItem);

        await RecalculateOrderTotalsAsync(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed order item {OrderItemId} from order {OrderId}", orderItemId, orderId);

        return true;
    }

    // CN039: Áp dụng voucher
    public async Task<OrderDto> ApplyVoucherAsync(int orderId, ApplyVoucherDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Voucher)
            .Include(o => o.Booking)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Không thể áp dụng voucher cho order đã thanh toán hoặc đã hủy");

        // Validate voucher
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code.ToUpper() == dto.VoucherCode.ToUpper() && v.IsActive);

        if (voucher == null)
            throw new ArgumentException("Voucher không tồn tại hoặc đã hết hiệu lực");

        // Validate voucher conditions
        int customerId = 0;
        if (order.Booking != null)
        {
            customerId = order.Booking.CustomerId;
        }

        var validationResult = await _voucherService.ValidateVoucherAsync(voucher.Code, customerId, order.Subtotal);

        if (!validationResult.IsValid)
            throw new InvalidOperationException(validationResult.Message);

        // Apply voucher
        order.VoucherId = voucher.Id;

        await RecalculateOrderTotalsAsync(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Applied voucher {VoucherCode} to order {OrderId}", dto.VoucherCode, orderId);

        return await MapToOrderDto(order);
    }

    // CN040: Xem tổng hóa đơn
    public async Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Voucher)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        var items = order.OrderItems.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            MenuItemId = oi.MenuItemId,
            MenuItemName = oi.MenuItem.Name,
            MenuItemImage = oi.MenuItem.ImageUrl,
            Quantity = oi.Quantity,
            Price = oi.Price,
            Subtotal = oi.Subtotal
        }).ToList();

        return new OrderSummaryDto
        {
            OrderId = order.Id,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            VoucherCode = order.Voucher?.Code,
            DepositAmount = order.DepositAmount,
            TotalAmount = order.Subtotal - order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Items = items
        };
    }

    // CN041: Thanh toán order
    public async Task<OrderPaymentResponseDto> ProcessPaymentAsync(int orderId, ProcessOrderPaymentDto dto, int staffId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => ProcessPaymentInTransactionAsync(orderId, dto, staffId));
    }

    private async Task<OrderPaymentResponseDto> ProcessPaymentInTransactionAsync(int orderId, ProcessOrderPaymentDto dto, int staffId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var order = await _context.Orders
            .FromSqlInterpolated($"SELECT * FROM \"Orders\" WHERE \"Id\" = {orderId} FOR UPDATE")
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Voucher)
            .Include(o => o.Booking)
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException("Order không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order đã được thanh toán hoặc đã hủy");

        if (order.OrderItems.Count == 0)
            throw new InvalidOperationException("Order chưa có món ăn nào");

        if (order.Booking == null)
        {
            order.Booking = await _context.Bookings
                .Where(b => b.TableId == order.TableId && b.Status == BookingStatus.CheckedIn)
                .OrderByDescending(b => b.CheckInTime)
                .FirstOrDefaultAsync();
            if (order.Booking != null)
            {
                order.BookingId = order.Booking.Id;
                order.DepositAmount = await _context.Payments
                    .Where(p => p.BookingId == order.Booking.Id && p.PaymentType == PaymentType.Deposit && p.Status == PaymentStatusEnum.Success)
                    .SumAsync(p => (decimal?)p.Amount) ?? order.DepositAmount;
            }
        }

        // Recalculate để đảm bảo đúng
        await RecalculateOrderTotalsAsync(order);

        // Tạo payment record
        var payment = new Payment
        {
            OrderId = orderId,
            BookingId = order.BookingId,
            Amount = order.FinalAmount,
            PaymentMethod = dto.PaymentMethod,
            PaymentType = order.DepositAmount > 0 ? PaymentType.Remaining : PaymentType.Full,
            Status = PaymentStatusEnum.Success,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // Update order status
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.PaymentMethod = dto.PaymentMethod;

        // Record voucher usage
        if (order.VoucherId.HasValue)
        {
            if (order.Booking?.CustomerId is not int customerId)
            {
                throw new InvalidOperationException("Voucher yêu cầu order gắn với khách hàng");
            }

            var voucherUsage = new VoucherUsage
            {
                VoucherId = order.VoucherId.Value,
                CustomerId = customerId,
                OrderId = orderId,
                BookingId = order.BookingId,
                UsedAt = DateTime.UtcNow
            };

            _context.VoucherUsages.Add(voucherUsage);

            var voucher = await _context.Vouchers.FindAsync(order.VoucherId.Value);
            if (voucher != null)
            {
                voucher.UsedCount++;
            }
        }

        // Auto check-out booking và giải phóng bàn
        if (order.Booking != null)
        {
            order.Booking.Status = BookingStatus.Completed;
            order.Booking.CheckOutTime = DateTime.UtcNow;
            order.Booking.PaymentStatus = PaymentStatus.Paid;
        }

        order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation("Processed payment for order {OrderId}, amount {Amount}", orderId, order.FinalAmount);

        var summary = await GetOrderSummaryAsync(orderId);

        return new OrderPaymentResponseDto
        {
            Success = true,
            Message = "Thanh toán thành công",
            OrderId = orderId,
            PaymentId = payment.Id,
            InvoiceUrl = null, // TODO: Generate PDF invoice
            OrderSummary = summary
        };
    }

    // Helper: Recalculate order totals
    private async Task RecalculateOrderTotalsAsync(Order order)
    {
        // Load items if not loaded
        if (!_context.Entry(order).Collection(o => o.OrderItems).IsLoaded)
        {
            await _context.Entry(order).Collection(o => o.OrderItems).LoadAsync();
        }

        // Calculate subtotal
        order.Subtotal = order.OrderItems.Sum(oi => oi.Subtotal);

        // Calculate discount
        if (order.VoucherId.HasValue)
        {
            if (!_context.Entry(order).Reference(o => o.Voucher).IsLoaded)
            {
                await _context.Entry(order).Reference(o => o.Voucher).LoadAsync();
            }

            var voucher = order.Voucher;
            if (voucher != null)
            {
                if (voucher.DiscountType == DiscountType.Percentage)
                {
                    order.DiscountAmount = order.Subtotal * voucher.DiscountValue / 100;
                }
                else
                {
                    order.DiscountAmount = voucher.DiscountValue;
                }
            }
        }
        else
        {
            order.DiscountAmount = 0;
        }

        // Calculate final amount
        order.FinalAmount = order.Subtotal - order.DiscountAmount - order.DepositAmount;

        if (order.FinalAmount < 0)
            order.FinalAmount = 0;
    }

    // Helper methods
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Table)
            .Include(o => o.Booking)
            .Include(o => o.Staff)
            .Include(o => o.Voucher)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return null;

        return await MapToOrderDto(order);
    }

    public async Task<List<OrderDto>> GetOrdersByBookingIdAsync(int bookingId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Table)
            .Include(o => o.Staff)
            .Include(o => o.Voucher)
            .Where(o => o.BookingId == bookingId)
            .ToListAsync();

        var result = new List<OrderDto>();
        foreach (var order in orders)
        {
            result.Add(await MapToOrderDto(order));
        }

        return result;
    }

    public async Task<List<OrderDto>> GetOrdersByTableIdAsync(int tableId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Table)
            .Include(o => o.Staff)
            .Include(o => o.Voucher)
            .Where(o => o.TableId == tableId && o.Status == OrderStatus.Pending)
            .ToListAsync();

        var result = new List<OrderDto>();
        foreach (var order in orders)
        {
            result.Add(await MapToOrderDto(order));
        }

        return result;
    }

    public async Task<bool> CancelOrderAsync(int orderId, int staffId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
            return false;

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể hủy order đang pending");

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled order {OrderId} by staff {StaffId}", orderId, staffId);

        return true;
    }

    public async Task<List<OrderDto>> GetOrderHistoryAsync(int? tableId, int? bookingId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.MenuItem)
            .Include(o => o.Booking)
            .Include(o => o.Table)
            .Where(o => (!tableId.HasValue || o.TableId == tableId) && (!bookingId.HasValue || o.BookingId == bookingId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        var result = new List<OrderDto>();
        foreach (var order in orders) result.Add(await MapToOrderDto(order));
        return result;
    }

    // Mapping helpers
    private async Task<OrderDto> MapToOrderDto(Order order)
    {
        // Ensure navigation properties are loaded
        if (!_context.Entry(order).Collection(o => o.OrderItems).IsLoaded)
            await _context.Entry(order).Collection(o => o.OrderItems).LoadAsync();

        if (!_context.Entry(order).Reference(o => o.Table).IsLoaded)
            await _context.Entry(order).Reference(o => o.Table).LoadAsync();

        if (order.BookingId.HasValue && !_context.Entry(order).Reference(o => o.Booking).IsLoaded)
            await _context.Entry(order).Reference(o => o.Booking).LoadAsync();

        if (order.StaffId.HasValue && !_context.Entry(order).Reference(o => o.Staff).IsLoaded)
            await _context.Entry(order).Reference(o => o.Staff).LoadAsync();

        if (order.VoucherId.HasValue && !_context.Entry(order).Reference(o => o.Voucher).IsLoaded)
            await _context.Entry(order).Reference(o => o.Voucher).LoadAsync();

        var items = new List<OrderItemDto>();
        foreach (var item in order.OrderItems)
        {
            items.Add(await MapToOrderItemDto(item));
        }

        return new OrderDto
        {
            Id = order.Id,
            BookingId = order.BookingId,
            BookingCode = order.Booking?.BookingCode,
            TableId = order.TableId,
            TableNumber = order.Table.TableNumber,
            StaffId = order.StaffId,
            StaffName = order.Staff?.FullName,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            VoucherId = order.VoucherId,
            VoucherCode = order.Voucher?.Code,
            DepositAmount = order.DepositAmount,
            FinalAmount = order.FinalAmount,
            PaymentMethod = order.PaymentMethod?.ToString(),
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Items = items
        };
    }

    private async Task<OrderItemDto> MapToOrderItemDto(OrderItem orderItem)
    {
        if (!_context.Entry(orderItem).Reference(oi => oi.MenuItem).IsLoaded)
            await _context.Entry(orderItem).Reference(oi => oi.MenuItem).LoadAsync();

        return new OrderItemDto
        {
            Id = orderItem.Id,
            MenuItemId = orderItem.MenuItemId,
            MenuItemName = orderItem.MenuItem.Name,
            MenuItemImage = orderItem.MenuItem.ImageUrl,
            Quantity = orderItem.Quantity,
            Price = orderItem.Price,
            Subtotal = orderItem.Subtotal
        };
    }
}
