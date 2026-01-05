using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

public class BookingFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public BookingStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? SearchTerm { get; set; } // Search by customer name, phone, booking code
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
