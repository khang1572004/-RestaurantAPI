using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Booking;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class TableAvailabilityService : ITableAvailabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TableAvailabilityService> _logger;

    public TableAvailabilityService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<TableAvailabilityService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(
        DateTime bookingDate,
        TimeSpan bookingTime,
        int numGuests)
    {
        // Get booking duration from config (default 2 hours)
        var durationHours = _configuration.GetValue<int>("BookingSettings:BookingDurationHours", 2);

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
            var hasConflict = await HasBookingConflictAsync(table.Id, bookingDate, bookingTime);

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
                    IsRecommended = table.Capacity >= numGuests && table.Capacity <= numGuests + 2 // Best fit
                });
            }
        }

        // Sort: Recommended first, then by capacity (ascending)
        return availableTables
            .OrderByDescending(t => t.IsRecommended)
            .ThenBy(t => t.Capacity)
            .ThenBy(t => t.TableNumber)
            .ToList();
    }

    public async Task<bool> IsTableAvailableAsync(
        int tableId,
        DateTime bookingDate,
        TimeSpan bookingTime,
        int? excludeBookingId = null)
    {
        var table = await _context.Tables.FindAsync(tableId);

        if (table == null || !table.IsActive || table.Status != TableStatus.Available)
        {
            return false;
        }

        return !await HasBookingConflictAsync(tableId, bookingDate, bookingTime, excludeBookingId);
    }

    public async Task<bool> HasBookingConflictAsync(
        int tableId,
        DateTime bookingDate,
        TimeSpan bookingTime,
        int? excludeBookingId = null)
    {
        // Get booking duration (default 2 hours)
        var durationHours = _configuration.GetValue<int>("BookingSettings:BookingDurationHours", 2);

        var requestedStart = bookingDate.Date.Add(bookingTime);
        var requestedEnd = requestedStart.AddHours(durationHours);

        // Get all active bookings for this table
        var activeBookings = await _context.Bookings
            .Where(b => b.TableId == tableId)
            .Where(b => b.Status != BookingStatus.Cancelled && 
                       b.Status != BookingStatus.Completed && 
                       b.Status != BookingStatus.NoShow)
            .Where(b => !excludeBookingId.HasValue || b.Id != excludeBookingId.Value)
            .Where(b => b.BookingDate.Date == bookingDate.Date) // Only check same day
            .ToListAsync();

        // Check for overlapping bookings in memory
        foreach (var booking in activeBookings)
        {
            var existingStart = booking.BookingDate.Date.Add(booking.BookingTime);
            var existingEnd = existingStart.AddHours(durationHours);

            // Check if time ranges overlap
            if (requestedStart < existingEnd && requestedEnd > existingStart)
            {
                return true; // Conflict found
            }
        }

        return false; // No conflict
    }
}