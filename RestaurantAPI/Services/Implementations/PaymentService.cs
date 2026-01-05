using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Payment;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IVNPayService _vnpayService;
    private readonly IMoMoService _momoService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext context,
        IVNPayService vnpayService,
        IMoMoService momoService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _vnpayService = vnpayService;
        _momoService = momoService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CreatePaymentResponse> CreateDepositPaymentAsync(
        CreateDepositPaymentDto dto,
        string ipAddress)
    {
        // ✅ FIX: Use execution strategy for transactions with retry
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ Validation: Check for existing pending payment (prevent double payment)
                var existingPendingPayment = await _context.Payments
                    .AnyAsync(p => p.BookingId == dto.BookingId 
                        && p.Status == PaymentStatusEnum.Pending);
                        
                if (existingPendingPayment)
                {
                    _logger.LogWarning("Duplicate payment attempt for booking {BookingId}", dto.BookingId);
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Đã có giao dịch đang chờ xử lý. Vui lòng chờ hoặc hủy giao dịch cũ."
                    };
                }

                // Get booking with related data
                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Table).ThenInclude(t => t.TableType)
                    .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

                if (booking == null)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy booking"
                    };
                }

                // ✅ Validation: Check booking status
                if (booking.Status == BookingStatus.Cancelled)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Booking đã bị hủy, không thể thanh toán"
                    };
                }

                if (booking.PaymentStatus == PaymentStatus.Paid)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Booking đã được thanh toán đầy đủ"
                    };
                }

                // ✅ Validation: Check booking date
                if (booking.BookingDate < DateTime.UtcNow.Date)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Không thể thanh toán cho booking trong quá khứ"
                    };
                }

                // Calculate deposit amount
                var depositAmount = booking.Table.CustomDepositAmount 
                    ?? booking.Table.TableType.DepositAmount;

                // ✅ Validation: Check amount
                if (depositAmount <= 0)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Số tiền thanh toán không hợp lệ"
                    };
                }

                // Create payment record
                var payment = new Payment
                {
                    BookingId = dto.BookingId,
                    Amount = depositAmount,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentType = PaymentType.Deposit,
                    Status = PaymentStatusEnum.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment {PaymentId} created for booking {BookingId}, amount: {Amount}", 
                    payment.Id, dto.BookingId, depositAmount);

                // Get return URL from config or DTO
                var returnUrl = dto.ReturnUrl 
                    ?? _configuration["VNPay:ReturnUrl"]
                    ?? throw new InvalidOperationException("ReturnUrl not configured");
                    
                var orderInfo = $"Thanh toan coc booking {booking.BookingCode}";

                string? paymentUrl = null;
                string? qrCodeUrl = null;

                // Generate payment URL based on method
                if (dto.PaymentMethod == PaymentMethod.VNPay)
                {
                    try
                    {
                        paymentUrl = _vnpayService.CreatePaymentUrl(
                            payment.Id,
                            depositAmount,
                            orderInfo,
                            returnUrl,
                            ipAddress
                        );
                        
                        _logger.LogInformation(
                            "VNPay payment URL generated for payment {PaymentId}", 
                            payment.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create VNPay payment URL");
                        await transaction.RollbackAsync();
                        
                        return new CreatePaymentResponse
                        {
                            Success = false,
                            Message = "Lỗi khi tạo liên kết thanh toán VNPay"
                        };
                    }
                }
                else if (dto.PaymentMethod == PaymentMethod.MoMo)
                {
                    var momoReturnUrl = _configuration["MoMo:ReturnUrl"]
                        ?? throw new InvalidOperationException("MoMo ReturnUrl not configured");
                        
                    var (success, message, url, qr) = await _momoService.CreatePaymentAsync(
                        payment.Id,
                        depositAmount,
                        orderInfo,
                        momoReturnUrl
                    );

                    if (!success)
                    {
                        _logger.LogError(
                            "Failed to create MoMo payment for payment {PaymentId}: {Message}", 
                            payment.Id, message);
                            
                        await transaction.RollbackAsync();
                        
                        return new CreatePaymentResponse
                        {
                            Success = false,
                            Message = message
                        };
                    }

                    paymentUrl = url;
                    qrCodeUrl = qr;
                    
                    _logger.LogInformation(
                        "MoMo payment URL generated for payment {PaymentId}", 
                        payment.Id);
                }
                else
                {
                    await transaction.RollbackAsync();
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Phương thức thanh toán không được hỗ trợ"
                    };
                }

                await transaction.CommitAsync();

                return new CreatePaymentResponse
                {
                    Success = true,
                    Message = "Tạo thanh toán thành công",
                    PaymentUrl = paymentUrl,
                    PaymentId = payment.Id,
                    QrCodeUrl = qrCodeUrl
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating deposit payment for booking {BookingId}", dto.BookingId);
                
                return new CreatePaymentResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi tạo thanh toán"
                };
            }
        });
    }

    public async Task<(bool Success, string Message, PaymentDto? Payment)> ProcessVNPayCallbackAsync(
        IQueryCollection queryParams)
    {
        // ✅ FIX: Use execution strategy
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("Processing VNPay callback: {QueryString}", queryParams.ToString());

                var isValid = _vnpayService.ValidateCallback(queryParams, out string transactionId, out decimal amount);

                if (!isValid)
                {
                    _logger.LogWarning("VNPay callback validation failed");
                    return (false, "Xác thực callback VNPay thất bại", null);
                }

                var paymentIdStr = queryParams["vnp_TxnRef"];
                if (!int.TryParse(paymentIdStr, out int paymentId))
                {
                    _logger.LogWarning("Invalid payment ID in VNPay callback: {PaymentId}", paymentIdStr);
                    return (false, "Payment ID không hợp lệ", null);
                }

                var payment = await _context.Payments
                    .Include(p => p.Booking).ThenInclude(b => b!.Customer)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
                    return (false, "Không tìm thấy payment", null);
                }

                // ✅ Idempotency check: Already processed
                if (payment.Status == PaymentStatusEnum.Success)
                {
                    _logger.LogInformation("Payment {PaymentId} already processed successfully", paymentId);
                    var existingDto = MapToDto(payment);
                    await transaction.CommitAsync();
                    return (true, "Giao dịch đã được xử lý trước đó", existingDto);
                }

                // ✅ Check amount matches
                if (payment.Amount != amount)
                {
                    _logger.LogWarning(
                        "Amount mismatch for payment {PaymentId}. Expected: {Expected}, Got: {Actual}",
                        paymentId, payment.Amount, amount);
                        
                    return (false, "Số tiền không khớp", null);
                }

                // Update payment
                payment.Status = PaymentStatusEnum.Success;
                payment.TransactionId = transactionId;
                payment.PaidAt = DateTime.UtcNow;
                payment.PaymentResponse = queryParams.ToString();

                // Update booking
                if (payment.Booking != null)
                {
                    payment.Booking.PaymentStatus = PaymentStatus.Paid;
                    payment.Booking.Status = BookingStatus.Confirmed;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "VNPay payment {PaymentId} processed successfully. Transaction: {TransactionId}", 
                    paymentId, transactionId);

                // ✅ Send email asynchronously (don't block the response)
                if (payment.Booking != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(
                                payment.Booking.Customer.Email,
                                "Xác nhận thanh toán cọc",
                                $@"
                                    <h2>Thanh toán thành công</h2>
                                    <p>Kính chào {payment.Booking.Customer.FullName},</p>
                                    <p>Booking <strong>{payment.Booking.BookingCode}</strong> đã thanh toán cọc thành công.</p>
                                    <p>Số tiền: <strong>{amount:N0}đ</strong></p>
                                    <p>Mã giao dịch: <strong>{transactionId}</strong></p>
                                    <p>Chúng tôi đã xác nhận booking của bạn.</p>
                                "
                            );
                            
                            _logger.LogInformation(
                                "Confirmation email sent for payment {PaymentId}", 
                                paymentId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, 
                                "Failed to send confirmation email for payment {PaymentId}", 
                                paymentId);
                        }
                    });
                }

                var paymentDto = MapToDto(payment);
                return (true, "Thanh toán thành công", paymentDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing VNPay callback");
                return (false, "Đã xảy ra lỗi khi xử lý callback", null);
            }
        });
    }

    public async Task<(bool Success, string Message, PaymentDto? Payment)> ProcessMoMoCallbackAsync(
        IQueryCollection queryParams)
    {
        // ✅ FIX: Use execution strategy
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("Processing MoMo callback: {QueryString}", queryParams.ToString());

                // ✅ Validate signature first
                var isValid = _momoService.ValidateCallback(queryParams, out string transactionId, out decimal amount);

                if (!isValid)
                {
                    _logger.LogWarning("MoMo callback validation failed");
                    return (false, "Xác thực callback MoMo thất bại", null);
                }

                // ✅ Extract paymentId from orderId
                // Format: PAYMENT_{paymentId}_{timestamp}
                // Example: PAYMENT_123_20260103123456
                var orderIdStr = queryParams["orderId"].ToString();
                
                if (string.IsNullOrEmpty(orderIdStr))
                {
                    _logger.LogWarning("OrderId is empty in MoMo callback");
                    return (false, "OrderId không hợp lệ", null);
                }

                var parts = orderIdStr.Split('_');
                
                if (parts.Length < 3)
                {
                    _logger.LogWarning("Invalid orderId format in MoMo callback: {OrderId}", orderIdStr);
                    return (false, "Format OrderId không đúng", null);
                }

                if (!int.TryParse(parts[1], out int paymentId))
                {
                    _logger.LogWarning("Cannot parse paymentId from orderId: {OrderId}", orderIdStr);
                    return (false, "Payment ID không hợp lệ", null);
                }

                _logger.LogInformation("Extracted paymentId: {PaymentId} from orderId: {OrderId}", 
                    paymentId, orderIdStr);

                // ✅ Get payment with related data
                var payment = await _context.Payments
                    .Include(p => p.Booking)
                        .ThenInclude(b => b!.Customer)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
                    return (false, "Không tìm thấy payment", null);
                }

                // ✅ Idempotency check - Already processed successfully
                if (payment.Status == PaymentStatusEnum.Success)
                {
                    _logger.LogInformation("Payment {PaymentId} already processed successfully", paymentId);
                    var existingDto = MapToDto(payment);
                    await transaction.CommitAsync();
                    return (true, "Giao dịch đã được xử lý trước đó", existingDto);
                }

                // ✅ Check if payment was already failed
                if (payment.Status == PaymentStatusEnum.Failed)
                {
                    _logger.LogWarning("Payment {PaymentId} was already marked as failed", paymentId);
                    return (false, "Payment đã bị đánh dấu thất bại trước đó", null);
                }

                // ✅ Validate amount matches
                if (payment.Amount != amount)
                {
                    _logger.LogWarning(
                        "Amount mismatch for payment {PaymentId}. Expected: {Expected}, Got: {Actual}",
                        paymentId, payment.Amount, amount);
                    
                    // Mark payment as failed
                    payment.Status = PaymentStatusEnum.Failed;
                    payment.PaymentResponse = queryParams.ToString();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return (false, "Số tiền không khớp", null);
                }

                // ✅ Check resultCode from MoMo
                var resultCode = queryParams["resultCode"].ToString();
                
                if (resultCode != "0")
                {
                    var message = queryParams["message"].ToString();
                    _logger.LogWarning(
                        "MoMo payment {PaymentId} failed with resultCode: {ResultCode}, message: {Message}",
                        paymentId, resultCode, message);
                    
                    // Mark payment as failed
                    payment.Status = PaymentStatusEnum.Failed;
                    payment.PaymentResponse = queryParams.ToString();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return (false, $"Thanh toán thất bại: {message}", null);
                }

                // ✅ Update payment status
                payment.Status = PaymentStatusEnum.Success;
                payment.TransactionId = transactionId;
                payment.PaidAt = DateTime.UtcNow;
                payment.PaymentResponse = queryParams.ToString();

                // ✅ Update booking status
                if (payment.Booking != null)
                {
                    payment.Booking.PaymentStatus = PaymentStatus.Paid;
                    payment.Booking.Status = BookingStatus.Confirmed;
                    
                    _logger.LogInformation(
                        "Updated booking {BookingId} status to Confirmed and PaymentStatus to Paid",
                        payment.Booking.Id);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "MoMo payment {PaymentId} processed successfully. Transaction: {TransactionId}", 
                    paymentId, transactionId);

                // ✅ Send confirmation email asynchronously (fire-and-forget)
                if (payment.Booking != null && !string.IsNullOrEmpty(payment.Booking.Customer?.Email))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(
                                payment.Booking.Customer.Email,
                                "Xác nhận thanh toán cọc",
                                $@"
                                    <h2>Thanh toán thành công qua MoMo</h2>
                                    <p>Kính chào {payment.Booking.Customer.FullName},</p>
                                    <p>Booking <strong>{payment.Booking.BookingCode}</strong> đã thanh toán cọc thành công.</p>
                                    <hr />
                                    <p><strong>Thông tin thanh toán:</strong></p>
                                    <ul>
                                        <li>Số tiền: <strong>{amount:N0}đ</strong></li>
                                        <li>Mã giao dịch: <strong>{transactionId}</strong></li>
                                        <li>Phương thức: <strong>MoMo</strong></li>
                                        <li>Thời gian: <strong>{DateTime.Now:dd/MM/yyyy HH:mm}</strong></li>
                                    </ul>
                                    <hr />
                                    <p>Chúng tôi đã xác nhận booking của bạn.</p>
                                    <p>Cảm ơn bạn đã sử dụng dịch vụ!</p>
                                "
                            );
                            
                            _logger.LogInformation(
                                "Confirmation email sent for payment {PaymentId}", 
                                paymentId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, 
                                "Failed to send confirmation email for payment {PaymentId}", 
                                paymentId);
                        }
                    });
                }

                var paymentDto = MapToDto(payment);
                return (true, "Thanh toán thành công", paymentDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing MoMo callback");
                return (false, "Đã xảy ra lỗi khi xử lý callback", null);
            }
        });
    }

    public async Task<List<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId)
    {
        var payments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return payments;
    }

    public async Task<List<PaymentDto>> GetPaymentsByCustomerIdAsync(int customerId)
    {
        var payments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking!.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return payments;
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == id);

        return payment != null ? MapToDto(payment) : null;
    }

    private static PaymentDto MapToDto(Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            BookingId = p.BookingId,
            BookingCode = p.Booking?.BookingCode,
            OrderId = p.OrderId,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod.ToString(),
            PaymentType = p.PaymentType.ToString(),
            Status = p.Status.ToString(),
            TransactionId = p.TransactionId,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        };
    }
}