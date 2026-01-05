using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ITableAvailabilityService _availabilityService;
    private readonly IVoucherService _voucherService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        ApplicationDbContext context,
        ITableAvailabilityService availabilityService,
        IVoucherService voucherService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<BookingService> logger)
    {
        _context = context;
        _availabilityService = availabilityService;
        _voucherService = voucherService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> CreateBookingAsync(
        CreateBookingDto dto,
        int customerId)
    {
        try
        {
            // Validate booking rules
            var isValidTime = await ValidateBookingRulesAsync(dto.BookingDate, dto.BookingTime);
            if (!isValidTime)
            {
                var minHours = _configuration.GetValue<int>("BookingSettings:MinAdvanceHours", 2);
                _logger.LogWarning("Booking validation failed: Not enough advance time for customer {CustomerId}", customerId);
                return (false, $"Bạn phải đặt trước ít nhất {minHours} giờ", null);
            }

            // Check table availability
            var isAvailable = await _availabilityService.IsTableAvailableAsync(
                dto.TableId, dto.BookingDate, dto.BookingTime);

            if (!isAvailable)
            {
                _logger.LogWarning("Table {TableId} is not available for {Date} {Time}", 
                    dto.TableId, dto.BookingDate, dto.BookingTime);
                return (false, "Bàn không khả dụng trong thời gian này", null);
            }

            _logger.LogInformation("Table {TableId} is available, proceeding with booking creation", dto.TableId);

            // Get table and customer info
            var table = await _context.Tables
                .Include(t => t.Zone)
                .Include(t => t.TableType)
                .FirstOrDefaultAsync(t => t.Id == dto.TableId);

            var customer = await _context.Users.FindAsync(customerId);

            if (table == null || customer == null)
            {
                return (false, "Không tìm thấy thông tin bàn hoặc khách hàng", null);
            }

            // Validate voucher if provided
            Voucher? voucher = null;
            if (!string.IsNullOrEmpty(dto.VoucherCode))
            {
                var voucherValidation = await _voucherService.ValidateVoucherAsync(
                    dto.VoucherCode, customerId);

                if (!voucherValidation.IsValid)
                {
                    return (false, voucherValidation.Message, null);
                }

                voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == dto.VoucherCode);
            }

            // Calculate deposit
            var depositAmount = table.CustomDepositAmount ?? table.TableType.DepositAmount;

            // Create booking
            var booking = new Booking
            {
                CustomerId = customerId,
                TableId = dto.TableId,
                BookingDate = dto.BookingDate,
                BookingTime = dto.BookingTime,
                NumGuests = dto.NumGuests,
                Notes = dto.Notes,
                BookingCode = GenerateBookingCode(),
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                VoucherId = voucher?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking created successfully: {BookingCode} by customer {CustomerId}", 
                booking.BookingCode, customerId);

            // Send confirmation email (don't block if fails)
            try
            {
                await _emailService.SendBookingConfirmationEmailAsync(
                    customer.Email,
                    customer.FullName,
                    booking.BookingCode,
                    booking.BookingDate,
                    booking.BookingTime
                );
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send confirmation email for booking {BookingCode}", booking.BookingCode);
            }

            var result = await GetBookingByIdAsync(booking.Id);

            return (true, $"Đặt bàn thành công! Mã đặt bàn: {booking.BookingCode}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for customer {CustomerId}", customerId);
            return (false, "Đã xảy ra lỗi khi đặt bàn", null);
        }
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(
        int bookingId,
        int customerId,
        string reason)
    {
        // ✅ Use execution strategy for retry logic
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerId);

                if (booking == null)
                {
                    return (false, "Không tìm thấy booking");
                }

                if (booking.Status == BookingStatus.Cancelled)
                {
                    return (false, "Booking đã bị hủy trước đó");
                }

                if (booking.Status == BookingStatus.Completed)
                {
                    return (false, "Không thể hủy booking đã hoàn thành");
                }

                // Check cancellation policy
                var cancellationHours = _configuration.GetValue<int>("BookingSettings:CancellationHours", 4);
                var bookingDateTime = booking.BookingDate.Date.Add(booking.BookingTime);
                var hoursUntilBooking = (bookingDateTime - DateTime.UtcNow).TotalHours;

                if (hoursUntilBooking < cancellationHours)
                {
                    return (false, $"Bạn phải hủy trước ít nhất {cancellationHours} giờ");
                }

                // ✅ Step 1: Cancel booking
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = reason;

                _logger.LogInformation(
                    "Cancelling booking {BookingId} with reason: {Reason}", 
                    bookingId, reason);

                // ✅ Step 2: Auto-cancel ALL pending payments
                var pendingPayments = await _context.Payments
                    .Where(p => p.BookingId == bookingId 
                        && p.Status == PaymentStatusEnum.Pending)
                    .ToListAsync();

                if (pendingPayments.Any())
                {
                    _logger.LogInformation(
                        "Found {Count} pending payments for booking {BookingId}, cancelling them",
                        pendingPayments.Count, bookingId);

                    foreach (var payment in pendingPayments)
                    {
                        payment.Status = PaymentStatusEnum.Cancelled;
                        
                        _logger.LogInformation(
                            "Auto-cancelled payment {PaymentId} (Method: {Method}, Amount: {Amount})",
                            payment.Id, payment.PaymentMethod, payment.Amount);
                    }
                }

                // ✅ Step 3: Create refund request if already paid
                if (booking.PaymentStatus == PaymentStatus.Paid || booking.PaymentStatus == PaymentStatus.Partial)
                {
                    var successfulPayment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.BookingId == bookingId 
                            && p.Status == PaymentStatusEnum.Success);

                    if (successfulPayment != null)
                    {
                        _logger.LogInformation(
                            "Booking {BookingId} has successful payment {PaymentId}, creating refund request",
                            bookingId, successfulPayment.Id);

                        var refundRequest = new RefundRequest
                        {
                            BookingId = bookingId,
                            PaymentId = successfulPayment.Id,
                            RefundAmount = successfulPayment.Amount,
                            Reason = reason,
                            Status = RefundStatus.Pending,
                            RequestedAt = DateTime.UtcNow
                        };

                        _context.RefundRequests.Add(refundRequest);

                        _logger.LogInformation(
                            "Created refund request for payment {PaymentId}, amount: {Amount}",
                            successfulPayment.Id, successfulPayment.Amount);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Booking {BookingId} marked as Paid but no successful payment found!",
                            bookingId);
                    }
                }

                // ✅ Step 4: Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Successfully cancelled booking {BookingId} by customer {CustomerId}. " +
                    "Cancelled {PendingCount} pending payments.",
                    bookingId, customerId, pendingPayments.Count);

                return (true, "Hủy booking thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, 
                    "Error cancelling booking {BookingId} by customer {CustomerId}", 
                    bookingId, customerId);
                return (false, "Đã xảy ra lỗi khi hủy booking");
            }
        });
    }

    public async Task<List<BookingDto>> GetCustomerBookingsAsync(int customerId, string? filter = null)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Table).ThenInclude(t => t.Zone)
            .Include(b => b.Table).ThenInclude(t => t.TableType)
            .Include(b => b.Voucher)
            .Where(b => b.CustomerId == customerId);

        // Apply filter
        if (!string.IsNullOrEmpty(filter))
        {
            var now = DateTime.UtcNow;
            query = filter.ToLower() switch
            {
                "upcoming" => query.Where(b => b.BookingDate >= now.Date && 
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)),
                "past" => query.Where(b => b.BookingDate < now.Date || b.Status == BookingStatus.Completed),
                "cancelled" => query.Where(b => b.Status == BookingStatus.Cancelled),
                _ => query
            };
        }

        var bookings = await query
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.BookingTime)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return bookings;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int id, int? customerId = null)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Table).ThenInclude(t => t.Zone)
            .Include(b => b.Table).ThenInclude(t => t.TableType)
            .Include(b => b.Voucher)
            .Where(b => b.Id == id);

        if (customerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == customerId.Value);
        }

        var booking = await query.FirstOrDefaultAsync();
        return booking != null ? MapToDto(booking) : null;
    }

    public async Task<List<BookingDto>> GetBookingsByDateAsync(DateTime date)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Table).ThenInclude(t => t.Zone)
            .Include(b => b.Table).ThenInclude(t => t.TableType)
            .Include(b => b.Voucher)
            .Where(b => b.BookingDate.Date == date.Date)
            .OrderBy(b => b.BookingTime)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return bookings;
    }

    public async Task<List<TableStatusDto>> GetTableStatusAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var tables = await _context.Tables
            .Include(t => t.Zone)
            .Include(t => t.Bookings)
            .Where(t => t.IsActive)
            .ToListAsync();

        var result = new List<TableStatusDto>();

        foreach (var table in tables)
        {
            var currentBooking = table.Bookings
                .Where(b => b.BookingDate.Date == today && 
                           b.Status == BookingStatus.CheckedIn)
                .OrderBy(b => b.BookingTime)
                .FirstOrDefault();

            result.Add(new TableStatusDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                ZoneName = table.Zone?.Name,
                Status = table.Status.ToString(),
                CurrentBookingCode = currentBooking?.BookingCode,
                CustomerName = currentBooking?.Customer.FullName,
                BookingTime = currentBooking != null 
                    ? currentBooking.BookingDate.Date.Add(currentBooking.BookingTime) 
                    : null,
                NumGuests = currentBooking?.NumGuests
            });
        }

        return result.OrderBy(t => t.ZoneName).ThenBy(t => t.TableNumber).ToList();
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> CheckInBookingAsync(string bookingCode)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking với mã này", null);
            }

            if (booking.Status == BookingStatus.CheckedIn)
            {
                return (false, "Booking đã check-in trước đó", null);
            }

            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
            {
                return (false, $"Không thể check-in booking có trạng thái {booking.Status}", null);
            }

            // Check time window (±30 minutes) - All times are in UTC
            var checkInWindow = _configuration.GetValue<int>("BookingSettings:CheckInWindowMinutes", 30);
            var bookingDateTimeUtc = booking.BookingDate.Date.Add(booking.BookingTime);
            var minutesUntilBooking = (bookingDateTimeUtc - DateTime.UtcNow).TotalMinutes;

            _logger.LogInformation(
                "Check-in validation - BookingTime: {BookingTime} UTC, CurrentTime: {CurrentTime} UTC, Difference: {Diff} minutes",
                bookingDateTimeUtc, DateTime.UtcNow, minutesUntilBooking);

            if (Math.Abs(minutesUntilBooking) > checkInWindow)
            {
                return (false, $"Chỉ có thể check-in trong khoảng ±{checkInWindow} phút so với giờ đặt", null);
            }

            // Update booking status
            booking.Status = BookingStatus.CheckedIn;
            booking.CheckInTime = DateTime.UtcNow;

            // Update table status
            booking.Table.Status = TableStatus.Occupied;

            await _context.SaveChangesAsync();

            var result = await GetBookingByIdAsync(booking.Id);

            _logger.LogInformation("Booking checked in: {BookingCode} at {CheckInTime} UTC", 
                bookingCode, booking.CheckInTime);
            
            return (true, "Check-in thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingCode}", bookingCode);
            return (false, "Đã xảy ra lỗi khi check-in", null);
        }
    }

    public async Task<(bool Success, string Message)> MarkAsNoShowAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            booking.Status = BookingStatus.NoShow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking marked as no-show: {BookingId}", bookingId);
            return (true, "Đánh dấu no-show thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking booking {BookingId} as no-show", bookingId);
            return (false, "Đã xảy ra lỗi");
        }
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> CreateWalkInBookingAsync(
        CreateWalkInDto dto,
        int staffId)
    {
        try
        {
            // Check if table is available
            var table = await _context.Tables.FindAsync(dto.TableId);
            if (table == null || table.Status != TableStatus.Available)
            {
                return (false, "Bàn không khả dụng", null);
            }

            // Find or create customer
            var customer = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == dto.Phone);

            if (customer == null)
            {
                customer = new User
                {
                    FullName = dto.FullName,
                    Email = $"{dto.Phone}@walkin.temp",
                    Phone = dto.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = UserRole.Customer,
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Create booking
            var booking = new Booking
            {
                CustomerId = customer.Id,
                TableId = dto.TableId,
                BookingDate = DateTime.UtcNow.Date,
                BookingTime = DateTime.UtcNow.TimeOfDay,
                NumGuests = dto.NumGuests,
                Notes = dto.Notes,
                BookingCode = GenerateBookingCode(),
                Status = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Unpaid,
                CheckInTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);

            // Update table status
            table.Status = TableStatus.Occupied;

            await _context.SaveChangesAsync();

            var result = await GetBookingByIdAsync(booking.Id);

            _logger.LogInformation("Walk-in booking created: {BookingCode} by staff {StaffId}", 
                booking.BookingCode, staffId);

            return (true, "Tạo walk-in booking thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating walk-in booking");
            return (false, "Đã xảy ra lỗi khi tạo walk-in booking", null);
        }
    }

    public async Task<(bool Success, string Message)> CheckOutBookingAsync(int bookingId)
    {
        try
        {
            // Step 1: Check validation with AsNoTracking
            var bookingCheck = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (bookingCheck == null)
            {
                return (false, "Không tìm thấy booking");
            }

            _logger.LogInformation(
                "CheckOut validation - BookingId: {BookingId}, Status: {Status} ({StatusInt})",
                bookingId, bookingCheck.Status, (int)bookingCheck.Status);

            if (bookingCheck.Status != BookingStatus.CheckedIn)
            {
                _logger.LogWarning(
                    "Cannot check-out booking {BookingId}. Current status: {Status} ({StatusInt})",
                    bookingId, bookingCheck.Status, (int)bookingCheck.Status);
                
                return (false, $"Chỉ có thể check-out booking đã check-in. Trạng thái hiện tại: {bookingCheck.Status}");
            }

            // Step 2: Load booking with tracking to update
            var booking = await _context.Bookings
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            // Step 3: Double check status
            if (booking.Status != BookingStatus.CheckedIn)
            {
                _context.ChangeTracker.Clear();
                return (false, $"Chỉ có thể check-out booking đã check-in. Trạng thái hiện tại: {booking.Status}");
            }

            // Update booking
            booking.Status = BookingStatus.Completed;
            booking.CheckOutTime = DateTime.UtcNow;

            // Release table
            booking.Table.Status = TableStatus.Available;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking checked out: {BookingId} at {CheckOutTime} UTC", 
                bookingId, booking.CheckOutTime);
            
            return (true, "Check-out thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", bookingId);
            _context.ChangeTracker.Clear();
            return (false, "Đã xảy ra lỗi khi check-out");
        }
    }

    // Admin operations
    public async Task<(List<BookingDto> Bookings, int TotalCount)> GetAllBookingsAsync(BookingFilterDto filter)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Table).ThenInclude(t => t.Zone)
            .Include(b => b.Table).ThenInclude(t => t.TableType)
            .Include(b => b.Voucher)
            .AsQueryable();

        // Apply filters
        if (filter.FromDate.HasValue)
        {
            query = query.Where(b => b.BookingDate >= filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(b => b.BookingDate <= filter.ToDate.Value.Date);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(b => b.Status == filter.Status.Value);
        }

        if (filter.PaymentStatus.HasValue)
        {
            query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(b => 
                b.Customer.FullName.ToLower().Contains(search) ||
                b.Customer.Phone!.Contains(search) ||
                b.BookingCode.ToLower().Contains(search)
            );
        }

        var totalCount = await query.CountAsync();

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return (bookings, totalCount);
    }

    public async Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            return (true, "Duyệt booking thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi");
        }
    }

    public async Task<(bool Success, string Message)> RejectBookingAsync(int bookingId, string reason)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = reason;

            await _context.SaveChangesAsync();

            return (true, "Từ chối booking thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi");
        }
    }

    public async Task<(bool Success, string Message)> DeleteBookingAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return (true, "Xóa booking thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi");
        }
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> UpdateBookingAsync(
        int bookingId,
        UpdateBookingDto dto)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking", null);
            }

            // Check table availability (exclude current booking)
            var isAvailable = await _availabilityService.IsTableAvailableAsync(
                dto.TableId, dto.BookingDate, dto.BookingTime, bookingId);

            if (!isAvailable)
            {
                return (false, "Bàn không khả dụng trong thời gian này", null);
            }

            booking.TableId = dto.TableId;
            booking.BookingDate = dto.BookingDate;
            booking.BookingTime = dto.BookingTime;
            booking.NumGuests = dto.NumGuests;
            booking.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var result = await GetBookingByIdAsync(bookingId);

            return (true, "Cập nhật booking thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi", null);
        }
    }

    // Helper methods
    public string GenerateBookingCode()
    {
        var code = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        return code;
    }

    public async Task<bool> ValidateBookingRulesAsync(DateTime bookingDate, TimeSpan bookingTime)
    {
        var minAdvanceHours = _configuration.GetValue<int>("BookingSettings:MinAdvanceHours", 2);
        var bookingDateTime = bookingDate.Date.Add(bookingTime);
        var hoursUntilBooking = (bookingDateTime - DateTime.UtcNow).TotalHours;

        return hoursUntilBooking >= minAdvanceHours;
    }

    private static BookingDto MapToDto(Booking b)
    {
        var table = b.Table;
        var depositAmount = table.CustomDepositAmount ?? table.TableType.DepositAmount;

        return new BookingDto
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer.FullName,
            CustomerEmail = b.Customer.Email,
            CustomerPhone = b.Customer.Phone,
            TableId = b.TableId,
            TableNumber = b.Table.TableNumber,
            ZoneName = b.Table.Zone?.Name,
            BookingDate = b.BookingDate,
            BookingTime = b.BookingTime,
            NumGuests = b.NumGuests,
            Notes = b.Notes,
            BookingCode = b.BookingCode,
            Status = b.Status.ToString(),
            PaymentStatus = b.PaymentStatus.ToString(),
            VoucherId = b.VoucherId,
            VoucherCode = b.Voucher?.Code,
            DepositAmount = depositAmount,
            CreatedAt = b.CreatedAt,
            CheckInTime = b.CheckInTime,
            CheckOutTime = b.CheckOutTime,
            CancelledAt = b.CancelledAt,
            CancellationReason = b.CancellationReason
        };
    }
}