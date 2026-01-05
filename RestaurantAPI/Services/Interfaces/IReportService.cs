using RestaurantAPI.DTOs.Report;

namespace RestaurantAPI.Services.Interfaces;

public interface IReportService
{
    // CN086: Báo cáo doanh thu
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "day");

    // CN087: Thống kê bookings
    Task<BookingReportDto> GetBookingReportAsync(DateTime fromDate, DateTime toDate);

    // CN088: Món ăn bán chạy
    Task<BestSellingReportDto> GetBestSellingReportAsync(DateTime fromDate, DateTime toDate, int topCount = 10);

    // CN089: Tỷ lệ lấp đầy bàn
    Task<OccupancyReportDto> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate);

    // CN090: Tỷ lệ hủy/No-show
    Task<CancellationReportDto> GetCancellationReportAsync(DateTime fromDate, DateTime toDate);

    // CN091: Export báo cáo
    Task<ExportReportResponseDto> ExportReportAsync(ExportReportDto dto);
}