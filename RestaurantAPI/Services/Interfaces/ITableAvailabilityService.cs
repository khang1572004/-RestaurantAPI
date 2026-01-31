using RestaurantAPI.DTOs.Booking;

namespace RestaurantAPI.Services.Interfaces;

public interface ITableAvailabilityService
{
    /// <summary>
    /// ✅ UPDATED: Get available tables with service duration parameter
    /// </summary>
    Task<List<AvailableTableDto>> GetAvailableTablesAsync(
        DateTime bookingDate, 
        TimeSpan bookingTime, 
        int numGuests,
        int serviceDurationMinutes = 120);

    /// <summary>
    /// ✅ UPDATED: Check if table is available with service duration
    /// </summary>
    Task<bool> IsTableAvailableAsync(
        int tableId, 
        DateTime bookingDate, 
        TimeSpan bookingTime, 
        int serviceDurationMinutes = 120,
        int? excludeBookingId = null);

    /// <summary>
    /// ✅ UPDATED: Check booking conflicts with service duration
    /// </summary>
    Task<bool> HasBookingConflictAsync(
        int tableId, 
        DateTime bookingDate, 
        TimeSpan bookingTime, 
        int serviceDurationMinutes = 120,
        int? excludeBookingId = null);

    /// <summary>
    /// ✅ NEW: Get next booking time after a given datetime
    /// </summary>
    Task<DateTime?> GetNextBookingTimeAsync(
        int tableId,
        DateTime afterDateTime);

    /// <summary>
    /// ✅ NEW: Calculate maximum walk-in duration considering next booking
    /// </summary>
    Task<int> CalculateMaxWalkInDurationAsync(
        int tableId, 
        DateTime walkInTime);

    /// <summary>
    /// ✅ NEW: Validate walk-in duration against next booking
    /// </summary>
    Task<bool> IsValidWalkInDurationAsync(
        int tableId,
        DateTime walkInTime,
        int requestedDurationMinutes);
}