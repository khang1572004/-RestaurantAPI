using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class UpdateBookingPolicyDto
{
    [Required]
    [Range(1, 48, ErrorMessage = "Thời gian đặt trước phải từ 1-48 giờ")]
    public int MinAdvanceHours { get; set; }

    [Required]
    [Range(1, 24, ErrorMessage = "Thời gian hủy phải từ 1-24 giờ")]
    public int CancellationHours { get; set; }

    [Required]
    public bool RequireDeposit { get; set; }

    [Range(0, 1000000)]
    public decimal? DefaultDepositAmount { get; set; }
}