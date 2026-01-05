using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Review;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Customer;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    // CN027: Đánh giá nhà hàng
    /// <summary>
    /// Tạo review cho booking đã hoàn thành (Customer only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var review = await _reviewService.CreateReviewAsync(dto, customerId);

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(
                review,
                "Cảm ơn bạn đã đánh giá!"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra khi tạo đánh giá"));
        }
    }

    // Xem reviews của mình
    /// <summary>
    /// Xem danh sách reviews của mình (Customer only)
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyReviews()
    {
        try
        {
            var customerId = GetCurrentUserId();
            var reviews = await _reviewService.GetMyReviewsAsync(customerId);

            return Ok(ApiResponse<List<MyReviewDto>>.SuccessResponse(
                reviews,
                "Lấy danh sách đánh giá thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my reviews");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Admin/Staff xem tất cả reviews
    /// <summary>
    /// Xem tất cả reviews (Admin/Staff only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetAllReviews()
    {
        try
        {
            var reviews = await _reviewService.GetAllReviewsAsync();

            return Ok(ApiResponse<List<ReviewDto>>.SuccessResponse(
                reviews,
                "Lấy danh sách đánh giá thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all reviews");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Xem review theo booking
    /// <summary>
    /// Xem review của một booking cụ thể
    /// </summary>
    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetReviewByBookingId(int bookingId)
    {
        try
        {
            var review = await _reviewService.GetReviewByBookingIdAsync(bookingId);

            if (review == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking này chưa có đánh giá"));

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(
                review,
                "Lấy đánh giá thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review by booking");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }

    // Kiểm tra có thể review không
    /// <summary>
    /// Kiểm tra xem có thể review booking này không
    /// </summary>
    [HttpGet("can-review/{bookingId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CanReview(int bookingId)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var canReview = await _reviewService.CanReviewAsync(bookingId, customerId);

            return Ok(ApiResponse<bool>.SuccessResponse(
                canReview,
                canReview ? "Có thể đánh giá" : "Không thể đánh giá"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking can review");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Có lỗi xảy ra"));
        }
    }
}