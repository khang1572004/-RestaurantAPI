using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Report;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CN086: Báo cáo doanh thu
    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "day")
    {
        var payments = await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Status == PaymentStatusEnum.Success && p.PaidAt >= fromDate && p.PaidAt < toDate.Date.AddDays(1))
            .ToListAsync();

        var orderPayments = payments.Where(p => p.OrderId.HasValue).ToList();
        var depositPayments = payments.Where(p => p.PaymentType == PaymentType.Deposit).ToList();
        var totalRevenue = orderPayments.Sum(p => p.Amount);
        var onlineRevenue = orderPayments.Where(p => p.PaymentMethod == PaymentMethod.VNPay || p.PaymentMethod == PaymentMethod.MoMo).Sum(p => p.Amount);
        var offlineRevenue = orderPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash || p.PaymentMethod == PaymentMethod.BankTransfer).Sum(p => p.Amount);
        var totalOrders = orderPayments.Select(p => p.OrderId).Distinct().Count();
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        // Group by period
        var chartData = new List<RevenueChartDataDto>();
        if (groupBy == "day")
        {
            chartData = orderPayments
                .GroupBy(p => p.PaidAt!.Value.Date)
                .Select(g => new RevenueChartDataDto
                {
                    Label = g.Key.ToString("dd/MM"),
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    OrderCount = g.Where(p => p.OrderId.HasValue).Select(p => p.OrderId).Distinct().Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }
        else if (groupBy == "week")
        {
            chartData = orderPayments
                .GroupBy(p => GetWeekOfYear(p.PaidAt!.Value))
                .Select(g => new RevenueChartDataDto
                {
                    Label = $"Tuần {g.Key}",
                    Date = orderPayments.Where(p => GetWeekOfYear(p.PaidAt!.Value) == g.Key).Min(p => p.PaidAt!.Value).Date,
                    Revenue = g.Sum(p => p.Amount),
                    OrderCount = g.Where(p => p.OrderId.HasValue).Select(p => p.OrderId).Distinct().Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }
        else if (groupBy == "month")
        {
            chartData = orderPayments
                .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt.Value.Month })
                .Select(g => new RevenueChartDataDto
                {
                    Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(p => p.Amount),
                    OrderCount = g.Where(p => p.OrderId.HasValue).Select(p => p.OrderId).Distinct().Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        var revenueByMethod = orderPayments
            .GroupBy(p => p.PaymentMethod.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        return new RevenueReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRevenue = totalRevenue,
            OnlineDepositRevenue = onlineRevenue + depositPayments.Where(p => p.PaymentMethod == PaymentMethod.VNPay || p.PaymentMethod == PaymentMethod.MoMo).Sum(p => p.Amount),
            OfflinePaymentRevenue = offlineRevenue,
            AverageOrderValue = averageOrderValue,
            TotalOrders = totalOrders,
            ChartData = chartData,
            RevenueByPaymentMethod = revenueByMethod
        };
    }

    // CN087: Báo cáo booking
    public async Task<BookingReportDto> GetBookingReportAsync(DateTime fromDate, DateTime toDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingDate >= fromDate && b.BookingDate <= toDate)
            .ToListAsync();

        var total = bookings.Count;
        var completed = bookings.Count(b => b.Status == BookingStatus.Completed);
        var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);
        var noShow = bookings.Count(b => b.Status == BookingStatus.NoShow);

        var completionRate = total > 0 ? (decimal)completed / total * 100 : 0;
        var cancellationRate = total > 0 ? (decimal)cancelled / total * 100 : 0;
        var noShowRate = total > 0 ? (decimal)noShow / total * 100 : 0;

        var avgGuests = bookings.Count > 0 ? (decimal)bookings.Average(b => b.NumGuests) : 0;

        var statusBreakdown = bookings
            .GroupBy(b => b.Status.ToString())
            .Select(g => new BookingStatusChartDto
            {
                Status = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? (decimal)g.Count() / total * 100 : 0
            })
            .ToList();

        var bookingsByDayOfWeek = bookings
            .GroupBy(b => b.BookingDate.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var bookingsByTimeSlot = bookings
            .GroupBy(b => GetTimeSlot(b.BookingTime))
            .ToDictionary(g => g.Key, g => g.Count());

        return new BookingReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalBookings = total,
            CompletedBookings = completed,
            CancelledBookings = cancelled,
            NoShowBookings = noShow,
            CompletionRate = completionRate,
            CancellationRate = cancellationRate,
            NoShowRate = noShowRate,
            AverageGuestsPerBooking = avgGuests,
            StatusBreakdown = statusBreakdown,
            BookingsByDayOfWeek = bookingsByDayOfWeek,
            BookingsByTimeSlot = bookingsByTimeSlot
        };
    }

    // CN088: Món ăn bán chạy
    public async Task<BestSellingReportDto> GetBestSellingReportAsync(DateTime fromDate, DateTime toDate, int topCount = 10)
    {
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.MenuItem)
                .ThenInclude(mi => mi.Category)
            .Where(oi => oi.Order.Status == OrderStatus.Paid && oi.Order.PaidAt >= fromDate && oi.Order.PaidAt <= toDate)
            .ToListAsync();

        var bestSelling = orderItems
            .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name, CategoryName = oi.MenuItem.Category.Name })
            .Select(g => new BestSellingItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                ItemName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Subtotal),
                AveragePrice = g.Average(oi => oi.Price),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(i => i.QuantitySold)
            .Take(topCount)
            .ToList();

        return new BestSellingReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Items = bestSelling
        };
    }

    // CN089: Tỷ lệ lấp đầy bàn
    public async Task<OccupancyReportDto> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate)
    {
        var totalDays = (toDate - fromDate).Days + 1;
        var tables = await _context.Tables.Include(t => t.Zone).ToListAsync();
        var totalTables = tables.Count;

        // Giả sử nhà hàng mở cửa 12 giờ/ngày (9:00-21:00)
        var hoursPerDay = 12;
        var totalAvailableHours = totalTables * totalDays * hoursPerDay;

        var bookings = await _context.Bookings
            .Where(b => b.BookingDate >= fromDate && b.BookingDate <= toDate)
            .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.CheckedIn)
            .ToListAsync();

        var totalBookedHours = bookings.Sum(b => b.ServiceDurationMinutes) / 60m;

        var occupancyRate = totalAvailableHours > 0 ? (decimal)totalBookedHours / totalAvailableHours * 100 : 0;

        var byZone = tables
            .GroupBy(t => new { t.ZoneId, t.Zone?.Name })
            .Select(g =>
            {
                var zoneAvailableHours = g.Count() * totalDays * hoursPerDay;
                var zoneBookedHours = bookings
                    .Where(b => tables.Any(t => t.Id == b.TableId && t.ZoneId == g.Key.ZoneId))
                    .Sum(b => b.ServiceDurationMinutes) / 60m;

                return new OccupancyByZoneDto
                {
                    ZoneId = g.Key.ZoneId,
                    ZoneName = g.Key.Name ?? "Không xác định",
                    TableCount = g.Count(),
                    AvailableHours = zoneAvailableHours,
                    BookedHours = zoneBookedHours,
                    OccupancyRate = zoneAvailableHours > 0 ? (decimal)zoneBookedHours / zoneAvailableHours * 100 : 0
                };
            })
            .ToList();

        var byDate = bookings
            .GroupBy(b => b.BookingDate.Date)
            .Select(g => new OccupancyByDateDto
            {
                Date = g.Key,
                BookingCount = g.Count(),
                OccupancyRate = totalTables > 0 ? (decimal)g.Count() / totalTables * 100 : 0
            })
            .OrderBy(d => d.Date)
            .ToList();

        var byDayOfWeek = bookings
            .GroupBy(b => b.BookingDate.DayOfWeek.ToString())
            .ToDictionary(
                g => g.Key,
                g => totalTables > 0 ? (decimal)g.Count() / totalTables / totalDays * 100 : 0
            );

        return new OccupancyReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            OverallOccupancyRate = occupancyRate,
            TotalTables = totalTables,
            TotalAvailableHours = totalAvailableHours,
            TotalBookedHours = totalBookedHours,
            ByZone = byZone,
            ByDate = byDate,
            ByDayOfWeek = byDayOfWeek
        };
    }

    // CN090: Tỷ lệ hủy/No-show
    public async Task<CancellationReportDto> GetCancellationReportAsync(DateTime fromDate, DateTime toDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingDate >= fromDate && b.BookingDate <= toDate)
            .ToListAsync();

        var total = bookings.Count;
        var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);
        var noShow = bookings.Count(b => b.Status == BookingStatus.NoShow);

        var cancellationRate = total > 0 ? (decimal)cancelled / total * 100 : 0;
        var noShowRate = total > 0 ? (decimal)noShow / total * 100 : 0;

        var cancellationReasons = bookings
            .Where(b => b.Status == BookingStatus.Cancelled && !string.IsNullOrEmpty(b.CancellationReason))
            .GroupBy(b => b.CancellationReason)
            .Select(g => new CancellationReasonDto
            {
                Reason = g.Key,
                Count = g.Count(),
                Percentage = cancelled > 0 ? (decimal)g.Count() / cancelled * 100 : 0
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        var cancelByDayOfWeek = bookings
            .Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.NoShow)
            .GroupBy(b => b.BookingDate.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var trend = bookings
            .GroupBy(b => b.BookingDate.Date)
            .Select(g => new CancellationTrendDto
            {
                Date = g.Key,
                TotalBookings = g.Count(),
                Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled),
                NoShow = g.Count(b => b.Status == BookingStatus.NoShow),
                CancellationRate = g.Count() > 0 ? (decimal)(g.Count(b => b.Status == BookingStatus.Cancelled) + g.Count(b => b.Status == BookingStatus.NoShow)) / g.Count() * 100 : 0
            })
            .OrderBy(t => t.Date)
            .ToList();

        return new CancellationReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalBookings = total,
            CancelledBookings = cancelled,
            NoShowBookings = noShow,
            CancellationRate = cancellationRate,
            NoShowRate = noShowRate,
            CancellationReasons = cancellationReasons,
            CancellationsByDayOfWeek = cancelByDayOfWeek,
            Trend = trend
        };
    }

    // CN091: Export report (Placeholder - cần thêm EPPlus/QuestPDF)
    public async Task<ExportReportResponseDto> ExportReportAsync(ExportReportDto dto)
    {
        // TODO: Implement Excel/PDF export với EPPlus/QuestPDF
        // For now, return placeholder
        _logger.LogWarning("Export report not yet implemented");

        return new ExportReportResponseDto
        {
            Success = false,
            Message = "Export chức năng đang được phát triển. Vui lòng sử dụng API để lấy dữ liệu.",
            FileUrl = null,
            FileName = null
        };
    }

    // Helper methods
    private int GetWeekOfYear(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private string GetTimeSlot(TimeSpan time)
    {
        var hour = time.Hours;
        if (hour < 12) return "Sáng (6-12h)";
        if (hour < 14) return "Trưa (12-14h)";
        if (hour < 18) return "Chiều (14-18h)";
        return "Tối (18-22h)";
    }
}
