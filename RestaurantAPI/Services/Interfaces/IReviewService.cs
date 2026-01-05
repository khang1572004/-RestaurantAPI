using RestaurantAPI.DTOs.Review;

namespace RestaurantAPI.Services.Interfaces;

public interface IReviewService
{
    // CN027: Tạo review cho booking đã hoàn thành
    Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, int customerId);

    // Xem review của mình
    Task<List<MyReviewDto>> GetMyReviewsAsync(int customerId);

    // Admin/Staff xem tất cả reviews
    Task<List<ReviewDto>> GetAllReviewsAsync();

    // Xem reviews theo booking
    Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId);

    // Kiểm tra có thể review không
    Task<bool> CanReviewAsync(int bookingId, int customerId);
}