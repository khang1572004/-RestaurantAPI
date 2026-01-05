using RestaurantAPI.DTOs.Booking;

namespace RestaurantAPI.Services.Interfaces;

public interface IBookingService
{
    // Customer operations
    Task<(bool Success, string Message, BookingDto? Data)> CreateBookingAsync(CreateBookingDto dto, int customerId);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int customerId, string reason);
    Task<List<BookingDto>> GetCustomerBookingsAsync(int customerId, string? filter = null);
    Task<BookingDto?> GetBookingByIdAsync(int id, int? customerId = null);
    
    // Staff operations
    Task<List<BookingDto>> GetBookingsByDateAsync(DateTime date);
    Task<List<TableStatusDto>> GetTableStatusAsync();
    Task<(bool Success, string Message, BookingDto? Data)> CheckInBookingAsync(string bookingCode);
    Task<(bool Success, string Message)> MarkAsNoShowAsync(int bookingId);
    Task<(bool Success, string Message, BookingDto? Data)> CreateWalkInBookingAsync(CreateWalkInDto dto, int staffId);
    Task<(bool Success, string Message)> CheckOutBookingAsync(int bookingId);
    
    // Admin operations
    Task<(List<BookingDto> Bookings, int TotalCount)> GetAllBookingsAsync(BookingFilterDto filter);
    Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId);
    Task<(bool Success, string Message)> RejectBookingAsync(int bookingId, string reason);
    Task<(bool Success, string Message)> DeleteBookingAsync(int bookingId);
    Task<(bool Success, string Message, BookingDto? Data)> UpdateBookingAsync(int bookingId, UpdateBookingDto dto);
    
    // Helper methods
    string GenerateBookingCode();
    Task<bool> ValidateBookingRulesAsync(DateTime bookingDate, TimeSpan bookingTime);
}