using RestaurantAPI.DTOs.Booking;

namespace RestaurantAPI.Services.Interfaces;

public interface IBookingService
{
    // ============================================
    // CUSTOMER OPERATIONS
    // ============================================
    Task<(bool Success, string Message, BookingDto? Data)> CreateBookingAsync(CreateBookingDto dto, int customerId);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int customerId, string reason);
    Task<List<BookingDto>> GetCustomerBookingsAsync(int customerId, string? filter = null);
    Task<BookingDto?> GetBookingByIdAsync(int id, int? customerId = null);

    // ============================================
    // STAFF OPERATIONS
    // ============================================
    Task<List<BookingDto>> GetBookingsByDateAsync(DateTime date);
    Task<List<TableStatusDto>> GetTableStatusAsync();
    Task<(bool Success, string Message, BookingDto? Data)> CheckInBookingAsync(string bookingCode);
    Task<(bool Success, string Message)> MarkAsNoShowAsync(int bookingId);
    Task<(bool Success, string Message, BookingDto? Data)> CreateWalkInBookingAsync(CreateWalkInDto dto, int staffId);
    Task<(bool Success, string Message)> CheckOutBookingAsync(int bookingId);

    // ✅ NEW: Walk-in availability check
    Task<WalkInAvailabilityResponse> CheckWalkInAvailabilityAsync(int tableId, DateTime walkInTime);

    // ✅ NEW: Late check-in (30-60 minutes after booking time)
    Task<(bool Success, string Message, BookingDto? Data)> LateCheckInAsync(
        string bookingCode,
        bool acknowledgeDepositForfeit);

    // ✅ NEW: Party booking approval
    Task<(bool Success, string Message)> ApprovePartyBookingAsync(int bookingId, string? adminNotes = null);
    Task<(bool Success, string Message)> RejectPartyBookingAsync(int bookingId, string reason);

    // ============================================
    // ADMIN OPERATIONS
    // ============================================
    Task<(List<BookingDto> Bookings, int TotalCount)> GetAllBookingsAsync(BookingFilterDto filter);
    Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId);
    Task<(bool Success, string Message)> RejectBookingAsync(int bookingId, string reason);
    Task<(bool Success, string Message)> CancelBookingByAdminAsync(int bookingId, string reason);
    Task<(bool Success, string Message)> DeleteBookingAsync(int bookingId);
    Task<(bool Success, string Message, BookingDto? Data)> UpdateBookingAsync(int bookingId, UpdateBookingDto dto);

    // ============================================
    // HELPER METHODS
    // ============================================
    string GenerateBookingCode();
    Task<bool> ValidateBookingRulesAsync(DateTime bookingDate, TimeSpan bookingTime);

    // ✅ NEW: Service duration helpers
    bool RequiresApproval(int serviceDurationMinutes);
    bool IsValidServiceDuration(int minutes, bool isWalkIn);
}
