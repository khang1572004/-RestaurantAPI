using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class TableAvailabilityService : ITableAvailabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TableAvailabilityService> _logger;

    // ✅ CONSTANTS
    private const int BUFFER_TIME_MINUTES = 15; // Cleanup buffer between bookings
    private const int WALK_IN_BUFFER_MINUTES = 60; // Walk-in must end 1h before next booking

    public TableAvailabilityService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<TableAvailabilityService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// ✅ UPDATED: Get available tables with service duration
    /// </summary>
    public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(
        DateTime bookingDate,
        TimeSpan bookingTime,
        int numGuests,
        int serviceDurationMinutes = 120)
    {
        // Get all active tables with sufficient capacity
        var tables = await _context.Tables
            .Include(t => t.Zone)
            .Include(t => t.TableType)
            .Where(t => t.IsActive && t.Capacity >= numGuests && t.Status == TableStatus.Available)
            .ToListAsync();

        var availableTables = new List<AvailableTableDto>();

        foreach (var table in tables)
        {
            // Check if table has conflicting bookings
            var hasConflict = await HasBookingConflictAsync(
                table.Id, 
                bookingDate, 
                bookingTime, 
                serviceDurationMinutes);

            if (!hasConflict)
            {
                var depositAmount = table.CustomDepositAmount ?? table.TableType.DepositAmount;

                availableTables.Add(new AvailableTableDto
                {
                    Id = table.Id,
                    TableNumber = table.TableNumber,
                    ZoneName = table.Zone?.Name,
                    TableTypeName = table.TableType.Name,
                    Capacity = table.Capacity,
                    DepositAmount = depositAmount,
                    IsRecommended = table.Capacity >= numGuests && table.Capacity <= numGuests + 2
                });
            }
        }

        return availableTables
            .OrderByDescending(t => t.IsRecommended)
            .ThenBy(t => t.Capacity)
            .ThenBy(t => t.TableNumber)
            .ToList();
    }

    /// <summary>
    /// ✅ UPDATED: Check if table is available with service duration
    /// </summary>
    public async Task<bool> IsTableAvailableAsync(
        int tableId,
        DateTime bookingDate,
        TimeSpan bookingTime,
        int serviceDurationMinutes = 120,
        int? excludeBookingId = null)
    {
        var table = await _context.Tables.FindAsync(tableId);

        if (table == null || !table.IsActive || table.Status != TableStatus.Available)
        {
            return false;
        }

        return !await HasBookingConflictAsync(
            tableId, 
            bookingDate, 
            bookingTime, 
            serviceDurationMinutes, 
            excludeBookingId);
    }

    /// <summary>
    /// ✅ UPDATED: Check booking conflicts with buffer time logic
    /// </summary>
    public async Task<bool> HasBookingConflictAsync(
        int tableId,
        DateTime bookingDate,
        TimeSpan bookingTime,
        int serviceDurationMinutes = 120,
        int? excludeBookingId = null)
    {
        // Calculate requested time range WITH BUFFER
        var requestedStart = bookingDate.Date.Add(bookingTime);
        var requestedEnd = requestedStart.AddMinutes(serviceDurationMinutes);
        var requestedBufferedEnd = requestedEnd.AddMinutes(BUFFER_TIME_MINUTES);

        _logger.LogInformation(
            "Checking conflict for Table {TableId}: {Start} - {End} (buffered to {BufferedEnd})",
            tableId,
            requestedStart.ToString("dd/MM/yyyy HH:mm"),
            requestedEnd.ToString("HH:mm"),
            requestedBufferedEnd.ToString("HH:mm"));

        // Get all active bookings for this table on the same date
        var activeBookings = await _context.Bookings
            .Where(b => b.TableId == tableId)
            .Where(b => b.Status != BookingStatus.Cancelled && 
                       b.Status != BookingStatus.Completed && 
                       b.Status != BookingStatus.NoShow)
            .Where(b => !excludeBookingId.HasValue || b.Id != excludeBookingId.Value)
            .Where(b => b.BookingDate.Date == bookingDate.Date)
            .ToListAsync();

        // Check for overlapping bookings
        foreach (var booking in activeBookings)
        {
            var existingStart = booking.BookingDate.Date.Add(booking.BookingTime);
            var existingEnd = existingStart.AddMinutes(booking.ServiceDurationMinutes);
            var existingBufferedEnd = existingEnd.AddMinutes(BUFFER_TIME_MINUTES);

            _logger.LogDebug(
                "Existing Booking {BookingId}: {Start} - {End} (buffered to {BufferedEnd})",
                booking.Id,
                existingStart.ToString("HH:mm"),
                existingEnd.ToString("HH:mm"),
                existingBufferedEnd.ToString("HH:mm"));

            // ✅ OVERLAP CHECK: requestedStart < existingBufferedEnd AND requestedBufferedEnd > existingStart
            if (requestedStart < existingBufferedEnd && requestedBufferedEnd > existingStart)
            {
                _logger.LogWarning(
                    "CONFLICT DETECTED: Requested ({ReqStart}-{ReqEnd}) overlaps with Booking {BookingId} ({ExStart}-{ExEnd})",
                    requestedStart.ToString("HH:mm"),
                    requestedBufferedEnd.ToString("HH:mm"),
                    booking.Id,
                    existingStart.ToString("HH:mm"),
                    existingBufferedEnd.ToString("HH:mm"));

                return true; // Conflict found
            }
        }

        _logger.LogInformation("No conflict found for Table {TableId}", tableId);
        return false; // No conflict
    }

    /// <summary>
    /// ✅ NEW: Get next booking time after a given datetime
    /// </summary>
    public async Task<DateTime?> GetNextBookingTimeAsync(int tableId, DateTime afterDateTime)
    {
        var nextBooking = await _context.Bookings
            .Where(b => b.TableId == tableId)
            .Where(b => b.Status != BookingStatus.Cancelled && 
                       b.Status != BookingStatus.Completed && 
                       b.Status != BookingStatus.NoShow)
            .Where(b => b.BookingDate.Date.Add(b.BookingTime) > afterDateTime)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.BookingTime)
            .FirstOrDefaultAsync();

        if (nextBooking == null)
            return null;

        return nextBooking.BookingDate.Date.Add(nextBooking.BookingTime);
    }

    /// <summary>
    /// ✅ NEW: Calculate max walk-in duration considering next booking
    /// </summary>
    public async Task<int> CalculateMaxWalkInDurationAsync(int tableId, DateTime walkInTime)
    {
        var nextBookingTime = await GetNextBookingTimeAsync(tableId, walkInTime);

        if (nextBookingTime == null)
        {
            // No next booking, return maximum allowed (10 hours)
            return 600; // 10 hours = 600 minutes
        }

        // Must end 1 hour before next booking
        var maxEndTime = nextBookingTime.Value.AddHours(-1);
        var maxDuration = (maxEndTime - walkInTime).TotalMinutes;

        // Minimum 60 minutes
        if (maxDuration < 60)
        {
            return 0; // Cannot walk-in
        }

        // Round down to nearest 30 minutes
        var roundedDuration = (int)(Math.Floor(maxDuration / 30) * 30);

        _logger.LogInformation(
            "Max walk-in duration for Table {TableId} at {WalkInTime}: {MaxDuration} minutes (next booking at {NextTime})",
            tableId, walkInTime.ToString("dd/MM/yyyy HH:mm"), roundedDuration, nextBookingTime.Value.ToString("HH:mm"));

        return roundedDuration;
    }

    /// <summary>
    /// ✅ NEW: Validate walk-in duration against next booking
    /// </summary>
    public async Task<bool> IsValidWalkInDurationAsync(
        int tableId,
        DateTime walkInTime,
        int requestedDurationMinutes)
    {
        var maxDuration = await CalculateMaxWalkInDurationAsync(tableId, walkInTime);

        if (maxDuration == 0)
        {
            _logger.LogWarning(
                "Walk-in not allowed for Table {TableId} at {WalkInTime} - too close to next booking",
                tableId, walkInTime.ToString("HH:mm"));
            return false;
        }

        var isValid = requestedDurationMinutes <= maxDuration;

        if (!isValid)
        {
            _logger.LogWarning(
                "Walk-in duration {Requested}m exceeds max {Max}m for Table {TableId}",
                requestedDurationMinutes, maxDuration, tableId);
        }

        return isValid;
    }
}