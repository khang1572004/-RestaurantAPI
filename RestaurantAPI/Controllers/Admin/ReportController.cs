using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Report;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    // CN086: Báo cáo doanh thu
    /// <summary>
    /// Báo cáo doanh thu theo thời gian
    /// </summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateRangeReportDto dto)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var report = await _reportService.GetRevenueReportAsync(
                dto.FromDate,
                dto.ToDate,
                dto.GroupBy ?? "day"
            );

            return Ok(ApiResponse<RevenueReportDto>.SuccessResponse(
                report,
                "Lấy báo cáo doanh thu thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN087: Báo cáo bookings
    /// <summary>
    /// Thống kê bookings
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] DateRangeReportDto dto)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var report = await _reportService.GetBookingReportAsync(dto.FromDate, dto.ToDate);

            return Ok(ApiResponse<BookingReportDto>.SuccessResponse(
                report,
                "Lấy báo cáo bookings thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN088: Món ăn bán chạy
    /// <summary>
    /// Báo cáo món ăn bán chạy nhất
    /// </summary>
    [HttpGet("best-selling")]
    public async Task<IActionResult> GetBestSellingReport([FromQuery] DateRangeReportDto dto, [FromQuery] int topCount = 10)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var report = await _reportService.GetBestSellingReportAsync(dto.FromDate, dto.ToDate, topCount);

            return Ok(ApiResponse<BestSellingReportDto>.SuccessResponse(
                report,
                "Lấy báo cáo món bán chạy thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best selling report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN089: Tỷ lệ lấp đầy bàn
    /// <summary>
    /// Báo cáo tỷ lệ lấp đầy bàn (occupancy rate)
    /// </summary>
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyReport([FromQuery] DateRangeReportDto dto)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var report = await _reportService.GetOccupancyReportAsync(dto.FromDate, dto.ToDate);

            return Ok(ApiResponse<OccupancyReportDto>.SuccessResponse(
                report,
                "Lấy báo cáo lấp đầy bàn thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting occupancy report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN090: Tỷ lệ hủy/No-show
    /// <summary>
    /// Báo cáo tỷ lệ hủy bàn và no-show
    /// </summary>
    [HttpGet("cancellation-rate")]
    public async Task<IActionResult> GetCancellationReport([FromQuery] DateRangeReportDto dto)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var report = await _reportService.GetCancellationReportAsync(dto.FromDate, dto.ToDate);

            return Ok(ApiResponse<CancellationReportDto>.SuccessResponse(
                report,
                "Lấy báo cáo hủy bàn thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancellation report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // CN091: Export báo cáo
    /// <summary>
    /// Export báo cáo ra Excel/PDF
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportDto dto)
    {
        try
        {
            if (dto.ToDate < dto.FromDate)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ngày kết thúc phải sau ngày bắt đầu"));
            }

            var result = await _reportService.ExportReportAsync(dto);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
            }

            return Ok(ApiResponse<ExportReportResponseDto>.SuccessResponse(
                result,
                result.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi export"));
        }
    }
}