using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Payment;

public class CreateDepositPaymentDto
{
    [Required(ErrorMessage = "Booking ID là bắt buộc")]
    public int BookingId { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    public PaymentMethod PaymentMethod { get; set; }

    public string? ReturnUrl { get; set; }
}