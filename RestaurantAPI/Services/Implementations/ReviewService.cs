using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Review;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CN027: Tạo review
    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, int customerId)
    {
        // Kiểm tra booking tồn tại
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

        if (booking == null)
            throw new ArgumentException("Booking không tồn tại");

        // Kiểm tra booking thuộc về customer
        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("Bạn không có quyền đánh giá booking này");

        // Kiểm tra booking đã hoàn thành
        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("Chỉ có thể đánh giá sau khi hoàn thành dùng bữa");

        // Kiểm tra đã review chưa
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookingId == dto.BookingId);

        if (existingReview != null)
            throw new InvalidOperationException("Bạn đã đánh giá booking này rồi");

        // Tạo review mới
        var review = new Review
        {
            CustomerId = customerId,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} created review for booking {BookingId}", customerId, dto.BookingId);

        return new ReviewDto
        {
            Id = review.Id,
            CustomerId = review.CustomerId,
            CustomerName = booking.Customer.FullName,
            BookingId = review.BookingId,
            BookingCode = booking.BookingCode,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    // Xem reviews của mình
    public async Task<List<MyReviewDto>> GetMyReviewsAsync(int customerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Booking)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyReviewDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                BookingCode = r.Booking.BookingCode,
                BookingDate = r.Booking.BookingDate,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return reviews;
    }

    // Admin/Staff xem tất cả reviews
    public async Task<List<ReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Booking)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                BookingId = r.BookingId,
                BookingCode = r.Booking.BookingCode,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return reviews;
    }

    // Xem review theo booking
    public async Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId)
    {
        var review = await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Booking)
            .Where(r => r.BookingId == bookingId)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                BookingId = r.BookingId,
                BookingCode = r.Booking.BookingCode,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        return review;
    }

    // Kiểm tra có thể review không
    public async Task<bool> CanReviewAsync(int bookingId, int customerId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return false;

        if (booking.CustomerId != customerId)
            return false;

        if (booking.Status != BookingStatus.Completed)
            return false;

        var existingReview = await _context.Reviews
            .AnyAsync(r => r.BookingId == bookingId);

        return !existingReview;
    }
}