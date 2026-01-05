using RestaurantAPI.DTOs.Booking;

namespace RestaurantAPI.Services.Interfaces;

public interface ITableAvailabilityService
{
    Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime bookingDate, TimeSpan bookingTime, int numGuests);
    Task<bool> IsTableAvailableAsync(int tableId, DateTime bookingDate, TimeSpan bookingTime, int? excludeBookingId = null);
    Task<bool> HasBookingConflictAsync(int tableId, DateTime bookingDate, TimeSpan bookingTime, int? excludeBookingId = null);
}