using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Services.Interfaces;
using System.Runtime.InteropServices;

namespace RestaurantAPI.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ITableAvailabilityService _availabilityService;
    private readonly IVoucherService _voucherService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingService> _logger;

    private static readonly TimeZoneInfo VietnamTimeZone = 
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

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

    private DateTime GetVietnamTime(DateTime utcTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
    }

    private DateTime GetCurrentVietnamTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
    }

    /// <summary>
    /// ✅ UPDATED: Create booking with service duration
    /// </summary>
    public async Task<(bool Success, string Message, BookingDto? Data)> CreateBookingAsync(
        CreateBookingDto dto,
        int customerId)
    {
        try
        {
            // ✅ VALIDATE SERVICE DURATION
            if (!IsValidServiceDuration(dto.ServiceDurationMinutes, false))
            {
                return (false, "Thời gian phục vụ không hợp lệ. Vui lòng chọn từ 1-6 giờ (bội số 30 phút)", null);
            }

            // ✅ VALIDATE BOOKING TIME IN VIETNAM TIMEZONE
            var currentVietnamTime = GetCurrentVietnamTime();
            var bookingDateTimeVN = dto.BookingDate.Date.Add(dto.BookingTime);
            
            var minHours = _configuration.GetValue<int>("BookingSettings:MinAdvanceHours", 2);
            var hoursUntilBooking = (bookingDateTimeVN - currentVietnamTime).TotalHours;

            _logger.LogInformation(
                "Booking validation - BookingTime: {BookingTime} VN, CurrentTime: {CurrentTime} VN, Hours until: {Hours}",
                bookingDateTimeVN.ToString("dd/MM/yyyy HH:mm"),
                currentVietnamTime.ToString("dd/MM/yyyy HH:mm"),
                hoursUntilBooking);

            if (hoursUntilBooking < minHours)
            {
                _logger.LogWarning(
                    "Booking validation failed: Not enough advance time. Need {MinHours}h, got {Hours}h",
                    minHours, hoursUntilBooking);
                
                return (false, $"Bạn phải đặt trước ít nhất {minHours} giờ", null);
            }

            // ✅ CHECK TABLE AVAILABILITY WITH SERVICE DURATION
            var isAvailable = await _availabilityService.IsTableAvailableAsync(
                dto.TableId, 
                dto.BookingDate, 
                dto.BookingTime,
                dto.ServiceDurationMinutes);

            if (!isAvailable)
            {
                _logger.LogWarning(
                    "Table {TableId} is not available for {Date} {Time} with {Duration} minutes service",
                    dto.TableId, dto.BookingDate, dto.BookingTime, dto.ServiceDurationMinutes);
                return (false, "Bàn không khả dụng trong thời gian này", null);
            }

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

            // Validate number of guests against table capacity
            if (dto.NumGuests > table.Capacity)
            {
                return (false, $"Bàn chỉ đủ chỗ cho tối đa {table.Capacity} khách", null);
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

            // ✅ DETERMINE INITIAL STATUS - Party booking needs approval
            var requiresApproval = RequiresApproval(dto.ServiceDurationMinutes);
            var initialStatus = requiresApproval ? BookingStatus.PendingApproval : BookingStatus.Pending;

            // ✅ CREATE BOOKING WITH SERVICE DURATION
            var booking = new Booking
            {
                CustomerId = customerId,
                TableId = dto.TableId,
                BookingDate = dto.BookingDate,
                BookingTime = dto.BookingTime,
                NumGuests = dto.NumGuests,
                Notes = dto.Notes,
                BookingCode = GenerateBookingCode(),
                Status = initialStatus,
                PaymentStatus = PaymentStatus.Unpaid,
                VoucherId = voucher?.Id,
                ServiceDurationMinutes = dto.ServiceDurationMinutes, // ✅ NEW
                IsLateCheckIn = false, // ✅ NEW
                DepositForfeited = false, // ✅ NEW
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Booking created: {BookingCode} by customer {CustomerId} for {BookingTime} VN, Duration: {Duration}m, Status: {Status}",
                booking.BookingCode, customerId, bookingDateTimeVN.ToString("dd/MM/yyyy HH:mm"), 
                dto.ServiceDurationMinutes, initialStatus);

            // ✅ SEND EMAIL BASED ON STATUS
            try
            {
                if (requiresApproval)
                {
                    await _emailService.SendEmailAsync(
                        customer.Email,
                        "Đặt bàn chờ duyệt - Party Booking",
                        $@"
                            <h2>Yêu cầu đặt bàn của bạn đang chờ duyệt</h2>
                            <p>Kính chào {customer.FullName},</p>
                            <p>Booking <strong>{booking.BookingCode}</strong> của bạn đang chờ xác nhận từ nhà hàng.</p>
                            <hr />
                            <p><strong>Thông tin booking:</strong></p>
                            <ul>
                                <li>Mã booking: <strong>{booking.BookingCode}</strong></li>
                                <li>Ngày: <strong>{booking.BookingDate:dd/MM/yyyy}</strong></li>
                                <li>Giờ: <strong>{booking.BookingTime:hh\\:mm}</strong></li>
                                <li>Số khách: <strong>{booking.NumGuests}</strong></li>
                                <li>Thời gian phục vụ: <strong>{dto.ServiceDurationMinutes / 60:F1} giờ</strong></li>
                            </ul>
                            <p>Đây là yêu cầu đặt bàn dài hạn (Party), chúng tôi sẽ xem xét và phản hồi sớm nhất.</p>
                            <p><em>Lưu ý: Bạn chỉ có thể thanh toán cọc sau khi booking được duyệt.</em></p>
                        ");
                }
                else
                {
                    await _emailService.SendBookingConfirmationEmailAsync(
                        customer.Email,
                        customer.FullName,
                        booking.BookingCode,
                        booking.BookingDate,
                        booking.BookingTime);
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send confirmation email for booking {BookingCode}", booking.BookingCode);
            }

            var result = await GetBookingByIdAsync(booking.Id);
            
            var successMessage = requiresApproval
                ? $"Đặt bàn thành công! Mã đặt bàn: {booking.BookingCode}. Yêu cầu của bạn đang chờ duyệt."
                : $"Đặt bàn thành công! Mã đặt bàn: {booking.BookingCode}";
            
            return (true, successMessage, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for customer {CustomerId}", customerId);
            return (false, "Đã xảy ra lỗi khi đặt bàn", null);
        }
    }

    /// <summary>
    /// ✅ UPDATED: Create walk-in booking with duration validation
    /// </summary>
    public async Task<(bool Success, string Message, BookingDto? Data)> CreateWalkInBookingAsync(
        CreateWalkInDto dto,
        int staffId)
    {
        try
        {
            // ✅ VALIDATE SERVICE DURATION FOR WALK-IN
            if (dto.ServiceDurationMinutes < 60 || dto.ServiceDurationMinutes % 30 != 0)
            {
                return (false, "Thời gian phục vụ không hợp lệ (tối thiểu 1 giờ, bội số 30 phút)", null);
            }

            var walkInTime = DateTime.UtcNow;

            // ✅ VALIDATE WALK-IN DURATION AGAINST NEXT BOOKING
            var isValidDuration = await _availabilityService.IsValidWalkInDurationAsync(
                dto.TableId,
                walkInTime,
                dto.ServiceDurationMinutes);

            if (!isValidDuration)
            {
                var maxDuration = await _availabilityService.CalculateMaxWalkInDurationAsync(dto.TableId, walkInTime);
                
                if (maxDuration == 0)
                {
                    return (false, "Không thể walk-in tại bàn này vì có booking kế tiếp quá gần (< 1 giờ)", null);
                }

                return (false, 
                    $"Thời gian phục vụ quá dài. Tối đa {maxDuration} phút ({maxDuration / 60:F1}h) do có booking kế tiếp", 
                    null);
            }

            // Check if table is available
            var table = await _context.Tables.FindAsync(dto.TableId);
            if (table == null || table.Status != TableStatus.Available)
            {
                return (false, "Bàn không khả dụng", null);
            }

            if (dto.NumGuests > table.Capacity)
            {
                return (false, $"Bàn chỉ đủ chỗ cho tối đa {table.Capacity} khách", null);
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

            // ✅ CREATE WALK-IN BOOKING WITH SERVICE DURATION
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
                ServiceDurationMinutes = dto.ServiceDurationMinutes, // ✅ NEW
                IsLateCheckIn = false, // ✅ NEW
                DepositForfeited = false, // ✅ NEW (walk-in no deposit)
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);

            // Update table status
            table.Status = TableStatus.Occupied;

            await _context.SaveChangesAsync();

            var result = await GetBookingByIdAsync(booking.Id);

            _logger.LogInformation(
                "Walk-in booking created: {BookingCode} by staff {StaffId}, Duration: {Duration}m",
                booking.BookingCode, staffId, dto.ServiceDurationMinutes);

            return (true, "Tạo walk-in booking thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating walk-in booking");
            return (false, "Đã xảy ra lỗi khi tạo walk-in booking", null);
        }
    }

    /// <summary>
    /// ✅ NEW: Check walk-in availability with max duration
    /// </summary>
    public async Task<WalkInAvailabilityResponse> CheckWalkInAvailabilityAsync(
        int tableId,
        DateTime walkInTime)
    {
        try
        {
            var table = await _context.Tables.FindAsync(tableId);

            if (table == null || !table.IsActive)
            {
                return new WalkInAvailabilityResponse
                {
                    CanWalkIn = false,
                    MaxServiceDurationMinutes = 0,
                    Message = "Bàn không tồn tại hoặc không hoạt động"
                };
            }

            if (table.Status != TableStatus.Available)
            {
                return new WalkInAvailabilityResponse
                {
                    CanWalkIn = false,
                    MaxServiceDurationMinutes = 0,
                    Message = $"Bàn đang ở trạng thái: {table.Status}"
                };
            }

            var maxDuration = await _availabilityService.CalculateMaxWalkInDurationAsync(tableId, walkInTime);
            var nextBookingTime = await _availabilityService.GetNextBookingTimeAsync(tableId, walkInTime);

            if (maxDuration == 0)
            {
                return new WalkInAvailabilityResponse
                {
                    CanWalkIn = false,
                    MaxServiceDurationMinutes = 0,
                    NextBookingTime = nextBookingTime,
                    Message = "Không thể walk-in vì có booking kế tiếp quá gần (< 1 giờ)"
                };
            }

            var suggestedEndTime = walkInTime.AddMinutes(maxDuration);

            return new WalkInAvailabilityResponse
            {
                CanWalkIn = true,
                MaxServiceDurationMinutes = maxDuration,
                SuggestedEndTime = suggestedEndTime,
                NextBookingTime = nextBookingTime,
                Message = $"Có thể phục vụ tối đa {maxDuration / 60:F1} giờ (đến {suggestedEndTime:HH:mm})"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking walk-in availability for table {TableId}", tableId);
            return new WalkInAvailabilityResponse
            {
                CanWalkIn = false,
                MaxServiceDurationMinutes = 0,
                Message = "Đã xảy ra lỗi khi kiểm tra"
            };
        }
    }

    /// <summary>
    /// ✅ UPDATED: Check-in with late check-in logic
    /// </summary>
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

            if (booking.Status == BookingStatus.Cancelled)
            {
                return (false, "Booking đã bị hủy", null);
            }

            if (booking.Status == BookingStatus.PendingApproval)
            {
                return (false, "Booking đang chờ duyệt, chưa thể check-in", null);
            }

            var checkInWindow = _configuration.GetValue<int>("BookingSettings:CheckInWindowMinutes", 30);
            var lateCheckInWindow = _configuration.GetValue<int>("BookingSettings:LateCheckInWindowMinutes", 60);
            
            var bookingDateTimeVN = booking.BookingDate.Date.Add(booking.BookingTime);
            var currentVietnamTime = GetCurrentVietnamTime();
            
            var minutesFromBookingTime = (currentVietnamTime - bookingDateTimeVN).TotalMinutes;

            _logger.LogInformation(
                "Check-in attempt - BookingTime: {BookingTime} VN, CurrentTime: {CurrentTime} VN, Difference: {Diff} minutes",
                bookingDateTimeVN.ToString("dd/MM/yyyy HH:mm"),
                currentVietnamTime.ToString("dd/MM/yyyy HH:mm"),
                minutesFromBookingTime);

            // ✅ NORMAL CHECK-IN WINDOW (±30 minutes)
            if (Math.Abs(minutesFromBookingTime) <= checkInWindow)
            {
                booking.Status = BookingStatus.CheckedIn;
                booking.CheckInTime = DateTime.UtcNow;
                booking.IsLateCheckIn = false;
                booking.DepositForfeited = false;
                booking.Table.Status = TableStatus.Occupied;

                await _context.SaveChangesAsync();

                var result = await GetBookingByIdAsync(booking.Id);

                _logger.LogInformation(
                    "Normal check-in successful: {BookingCode} at {CheckInTime} VN",
                    bookingCode, GetVietnamTime(booking.CheckInTime.Value).ToString("dd/MM/yyyy HH:mm:ss"));
                
                return (true, "Check-in thành công", result);
            }

            // ✅ LATE CHECK-IN WINDOW (30-60 minutes late)
            if (minutesFromBookingTime > checkInWindow && minutesFromBookingTime <= lateCheckInWindow)
            {
                // This is late check-in - requires explicit confirmation
                return (false, 
                    $"Bạn đến muộn {minutesFromBookingTime:F0} phút. Vui lòng sử dụng endpoint /late-check-in để xác nhận (tiền cọc sẽ bị mất).", 
                    null);
            }

            // ✅ TOO LATE (> 60 minutes)
            if (minutesFromBookingTime > lateCheckInWindow)
            {
                _logger.LogWarning(
                    "Check-in too late: {BookingCode}, {Minutes} minutes after booking time",
                    bookingCode, minutesFromBookingTime);
                
                return (false, 
                    "Đã quá thời gian check-in cho phép (> 1 giờ). Booking đã bị hủy.", 
                    null);
            }

            // ✅ TOO EARLY
            _logger.LogWarning(
                "Check-in too early: {BookingCode}, {Minutes} minutes before booking time",
                bookingCode, Math.Abs(minutesFromBookingTime));
            
            return (false, 
                $"Chỉ có thể check-in trong khoảng ±{checkInWindow} phút so với giờ đặt. " +
                $"Giờ đặt: {bookingDateTimeVN:dd/MM/yyyy HH:mm}, " +
                $"Giờ hiện tại: {currentVietnamTime:dd/MM/yyyy HH:mm}", 
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingCode}", bookingCode);
            return (false, "Đã xảy ra lỗi khi check-in", null);
        }
    }

    /// <summary>
    /// ✅ NEW: Late check-in with deposit forfeit acknowledgment
    /// </summary>
    public async Task<(bool Success, string Message, BookingDto? Data)> LateCheckInAsync(
        string bookingCode,
        bool acknowledgeDepositForfeit)
    {
        try
        {
            if (!acknowledgeDepositForfeit)
            {
                return (false, "Bạn phải xác nhận chấp nhận mất tiền cọc để check-in muộn", null);
            }

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

            var lateCheckInWindow = _configuration.GetValue<int>("BookingSettings:LateCheckInWindowMinutes", 60);
            var bookingDateTimeVN = booking.BookingDate.Date.Add(booking.BookingTime);
            var currentVietnamTime = GetCurrentVietnamTime();
            var minutesFromBookingTime = (currentVietnamTime - bookingDateTimeVN).TotalMinutes;

            // ✅ VALIDATE LATE CHECK-IN WINDOW
            if (minutesFromBookingTime <= 30)
            {
                return (false, "Bạn chưa muộn, vui lòng sử dụng check-in thông thường", null);
            }

            if (minutesFromBookingTime > lateCheckInWindow)
            {
                return (false, $"Đã quá thời gian check-in cho phép (> {lateCheckInWindow} phút)", null);
            }

            // ✅ PROCESS LATE CHECK-IN
            booking.Status = BookingStatus.CheckedIn;
            booking.CheckInTime = DateTime.UtcNow;
            booking.IsLateCheckIn = true;
            booking.DepositForfeited = true;
            booking.Table.Status = TableStatus.Occupied;

            await _context.SaveChangesAsync();

            var result = await GetBookingByIdAsync(booking.Id);

            _logger.LogWarning(
                "LATE check-in: {BookingCode} at {CheckInTime} VN, {Minutes} minutes late, DEPOSIT FORFEITED",
                bookingCode, 
                GetVietnamTime(booking.CheckInTime.Value).ToString("dd/MM/yyyy HH:mm:ss"),
                minutesFromBookingTime);

            return (true, 
                $"Check-in thành công. Lưu ý: Tiền cọc đã bị mất do check-in muộn {minutesFromBookingTime:F0} phút.", 
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing late check-in for {BookingCode}", bookingCode);
            return (false, "Đã xảy ra lỗi khi xử lý late check-in", null);
        }
    }

    /// <summary>
    /// ✅ NEW: Approve party booking
    /// </summary>
    public async Task<(bool Success, string Message)> ApprovePartyBookingAsync(int bookingId, string? adminNotes = null)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            if (booking.Status != BookingStatus.PendingApproval)
            {
                return (false, $"Booking không ở trạng thái chờ duyệt. Trạng thái hiện tại: {booking.Status}");
            }

            booking.Status = BookingStatus.Confirmed;
            if (!string.IsNullOrEmpty(adminNotes))
            {
                booking.Notes = (booking.Notes ?? "") + $"\n[Admin Note]: {adminNotes}";
            }

            await _context.SaveChangesAsync();

            // ✅ SEND APPROVAL EMAIL
            try
            {
                await _emailService.SendEmailAsync(
                    booking.Customer.Email,
                    "Booking đã được duyệt",
                    $@"
                        <h2>Booking của bạn đã được xác nhận!</h2>
                        <p>Kính chào {booking.Customer.FullName},</p>
                        <p>Booking <strong>{booking.BookingCode}</strong> đã được duyệt.</p>
                        <p>Bạn có thể tiến hành thanh toán cọc để hoàn tất đặt bàn.</p>
                        <hr />
                        <p><strong>Thông tin booking:</strong></p>
                        <ul>
                            <li>Ngày: <strong>{booking.BookingDate:dd/MM/yyyy}</strong></li>
                            <li>Giờ: <strong>{booking.BookingTime:hh\\:mm}</strong></li>
                            <li>Thời gian phục vụ: <strong>{booking.ServiceDurationMinutes / 60:F1} giờ</strong></li>
                        </ul>
                    ");
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send approval email for booking {BookingId}", bookingId);
            }

            _logger.LogInformation("Party booking approved: {BookingId}", bookingId);
            return (true, "Duyệt booking thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving party booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi khi duyệt booking");
        }
    }

    /// <summary>
    /// ✅ NEW: Reject party booking
    /// </summary>
    public async Task<(bool Success, string Message)> RejectPartyBookingAsync(int bookingId, string reason)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            if (booking.Status != BookingStatus.PendingApproval)
            {
                return (false, $"Booking không ở trạng thái chờ duyệt. Trạng thái hiện tại: {booking.Status}");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = $"[REJECTED]: {reason}";

            await _context.SaveChangesAsync();

            // ✅ SEND REJECTION EMAIL
            try
            {
                await _emailService.SendEmailAsync(
                    booking.Customer.Email,
                    "Booking không được duyệt",
                    $@"
                        <h2>Booking không được xác nhận</h2>
                        <p>Kính chào {booking.Customer.FullName},</p>
                        <p>Rất tiếc, booking <strong>{booking.BookingCode}</strong> không được duyệt.</p>
                        <p><strong>Lý do:</strong> {reason}</p>
                        <p>Vui lòng liên hệ nhà hàng để biết thêm chi tiết hoặc đặt lại bàn với thời gian phù hợp hơn.</p>
                    ");
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send rejection email for booking {BookingId}", bookingId);
            }

            _logger.LogInformation("Party booking rejected: {BookingId}, Reason: {Reason}", bookingId, reason);
            return (true, "Từ chối booking thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting party booking {BookingId}", bookingId);
            return (false, "Đã xảy ra lỗi khi từ chối booking");
        }
    }

    /// <summary>
    /// ✅ HELPER: Check if booking requires approval
    /// </summary>
    public bool RequiresApproval(int serviceDurationMinutes)
    {
        return serviceDurationMinutes > 240; // > 4 hours
    }

    /// <summary>
    /// ✅ HELPER: Validate service duration
    /// </summary>
    public bool IsValidServiceDuration(int minutes, bool isWalkIn)
    {
        // Must be multiple of 30 minutes
        if (minutes % 30 != 0 || minutes < 60)
            return false;

        if (isWalkIn)
        {
            // Walk-in can be longer, but validated against next booking
            return minutes <= 600; // Max 10 hours
        }
        else
        {
            // Online booking max 6 hours
            return minutes <= 360;
        }
    }

    // ============================================
    // EXISTING METHODS (KEPT AS IS)
    // ============================================

    public async Task<(bool Success, string Message)> CancelBookingAsync(
        int bookingId,
        int customerId,
        string reason)
    {
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

                var cancellationHours = _configuration.GetValue<int>("BookingSettings:CancellationHours", 4);
                var bookingDateTime = booking.BookingDate.Date.Add(booking.BookingTime);
                var hoursUntilBooking = (bookingDateTime - DateTime.UtcNow).TotalHours;

                if (hoursUntilBooking < cancellationHours)
                {
                    return (false, $"Bạn phải hủy trước ít nhất {cancellationHours} giờ");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = reason;

                _logger.LogInformation(
                    "Cancelling booking {BookingId} with reason: {Reason}", 
                    bookingId, reason);

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

        if (!string.IsNullOrEmpty(filter))
        {
            var now = DateTime.UtcNow;
            query = filter.ToLower() switch
            {
                "upcoming" => query.Where(b => b.BookingDate >= now.Date && 
                    (b.Status == BookingStatus.Pending || 
                     b.Status == BookingStatus.PendingApproval ||
                     b.Status == BookingStatus.Confirmed)),
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
            booking.DepositForfeited = true; // ✅ NoShow loses deposit
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking marked as no-show: {BookingId}, deposit forfeited", bookingId);
            return (true, "Đánh dấu no-show thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking booking {BookingId} as no-show", bookingId);
            return (false, "Đã xảy ra lỗi");
        }
    }

    public async Task<(bool Success, string Message)> CheckOutBookingAsync(int bookingId)
    {
        try
        {
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

            var booking = await _context.Bookings
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy booking");
            }

            if (booking.Status != BookingStatus.CheckedIn)
            {
                _context.ChangeTracker.Clear();
                return (false, $"Chỉ có thể check-out booking đã check-in. Trạng thái hiện tại: {booking.Status}");
            }

            booking.Status = BookingStatus.Completed;
            booking.CheckOutTime = DateTime.UtcNow;
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

    public async Task<(List<BookingDto> Bookings, int TotalCount)> GetAllBookingsAsync(BookingFilterDto filter)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Table).ThenInclude(t => t.Zone)
            .Include(b => b.Table).ThenInclude(t => t.TableType)
            .Include(b => b.Voucher)
            .AsQueryable();

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

            // ✅ UPDATED: Check availability with service duration
            var isAvailable = await _availabilityService.IsTableAvailableAsync(
                dto.TableId, 
                dto.BookingDate, 
                dto.BookingTime, 
                dto.ServiceDurationMinutes,
                bookingId);

            if (!isAvailable)
            {
                return (false, "Bàn không khả dụng trong thời gian này", null);
            }

            booking.TableId = dto.TableId;
            booking.BookingDate = dto.BookingDate;
            booking.BookingTime = dto.BookingTime;
            booking.NumGuests = dto.NumGuests;
            booking.Notes = dto.Notes;
            booking.ServiceDurationMinutes = dto.ServiceDurationMinutes; // ✅ NEW

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

    public string GenerateBookingCode()
    {
        var code = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        return code;
    }

    public async Task<bool> ValidateBookingRulesAsync(DateTime bookingDate, TimeSpan bookingTime)
    {
        var minAdvanceHours = _configuration.GetValue<int>("BookingSettings:MinAdvanceHours", 2);
        var currentVietnamTime = GetCurrentVietnamTime();
        var bookingDateTimeVN = bookingDate.Date.Add(bookingTime);
        var hoursUntilBooking = (bookingDateTimeVN - currentVietnamTime).TotalHours;

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
            CancellationReason = b.CancellationReason,
            // ✅ NEW FIELDS
            ServiceDurationMinutes = b.ServiceDurationMinutes,
            IsLateCheckIn = b.IsLateCheckIn,
            DepositForfeited = b.DepositForfeited,
            ActualEndTime = b.BookingDate.Date.Add(b.BookingTime).AddMinutes(b.ServiceDurationMinutes)
        };
    }
}