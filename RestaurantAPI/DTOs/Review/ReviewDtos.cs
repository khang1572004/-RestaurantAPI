using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Review;

// DTO để tạo review mới
public class CreateReviewDto
{
    [Required(ErrorMessage = "Booking ID là bắt buộc")]
    public int BookingId { get; set; }

    [Required(ErrorMessage = "Đánh giá là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1-5 sao")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự")]
    public string? Comment { get; set; }
}

// DTO response cho review
public class ReviewDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

// DTO cho danh sách review của customer
public class MyReviewDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}