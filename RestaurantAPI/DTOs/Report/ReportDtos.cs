using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Report;

// DTO request cho báo cáo theo thời gian
public class DateRangeReportDto
{
    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    public DateTime FromDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    public DateTime ToDate { get; set; }

    public string? GroupBy { get; set; } = "day"; // day, week, month
}

// DTO cho báo cáo doanh thu (CN086)
public class RevenueReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OnlineDepositRevenue { get; set; }
    public decimal OfflinePaymentRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
    public List<RevenueChartDataDto> ChartData { get; set; } = new();
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
}

public class RevenueChartDataDto
{
    public string Label { get; set; } = string.Empty; // Date or period label
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

// DTO cho báo cáo booking (CN087)
public class BookingReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int NoShowBookings { get; set; }
    public decimal CompletionRate { get; set; } // %
    public decimal CancellationRate { get; set; } // %
    public decimal NoShowRate { get; set; } // %
    public decimal AverageGuestsPerBooking { get; set; }
    public List<BookingStatusChartDto> StatusBreakdown { get; set; } = new();
    public Dictionary<string, int> BookingsByDayOfWeek { get; set; } = new();
    public Dictionary<string, int> BookingsByTimeSlot { get; set; } = new();
}

public class BookingStatusChartDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

// DTO cho món ăn bán chạy (CN088)
public class BestSellingReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<BestSellingItemDto> Items { get; set; } = new();
}

public class BestSellingItemDto
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public int OrderCount { get; set; } // Số order có món này
}

// DTO cho tỷ lệ lấp đầy bàn (CN089)
public class OccupancyReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OverallOccupancyRate { get; set; } // %
    public int TotalTables { get; set; }
    public decimal TotalAvailableHours { get; set; }
    public decimal TotalBookedHours { get; set; }
    public List<OccupancyByZoneDto> ByZone { get; set; } = new();
    public List<OccupancyByDateDto> ByDate { get; set; } = new();
    public Dictionary<string, decimal> ByDayOfWeek { get; set; } = new();
}

public class OccupancyByZoneDto
{
    public int? ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public int TableCount { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AvailableHours { get; set; }
    public decimal BookedHours { get; set; }
}

public class OccupancyByDateDto
{
    public DateTime Date { get; set; }
    public decimal OccupancyRate { get; set; }
    public int BookingCount { get; set; }
}

// DTO cho tỷ lệ hủy/No-show (CN090)
public class CancellationReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int NoShowBookings { get; set; }
    public decimal CancellationRate { get; set; } // %
    public decimal NoShowRate { get; set; } // %
    public List<CancellationReasonDto> CancellationReasons { get; set; } = new();
    public Dictionary<string, int> CancellationsByDayOfWeek { get; set; } = new();
    public List<CancellationTrendDto> Trend { get; set; } = new();
}

public class CancellationReasonDto
{
    public string? Reason { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class CancellationTrendDto
{
    public DateTime Date { get; set; }
    public int TotalBookings { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public decimal CancellationRate { get; set; }
}

// DTO cho export (CN091)
public class ExportReportDto
{
    [Required]
    public string ReportType { get; set; } = string.Empty; // revenue, bookings, best-selling, etc.

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    public string Format { get; set; } = "excel"; // excel, pdf
}

public class ExportReportResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
}