using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Booking;

// Check walk-in availability
public class CheckWalkInAvailabilityDto
{
    [Required]
    public int TableId { get; set; }

    [Required]
    public DateTime WalkInTime { get; set; }
}

public class WalkInAvailabilityResponse
{
    public bool CanWalkIn { get; set; }
    public int MaxServiceDurationMinutes { get; set; }
    public DateTime? SuggestedEndTime { get; set; }
    public DateTime? NextBookingTime { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Late check-in DTO
public class LateCheckInDto
{
    [Required(ErrorMessage = "Mã đặt bàn là bắt buộc")]
    public string BookingCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bạn phải xác nhận chấp nhận mất tiền cọc")]
    public bool AcknowledgeDepositForfeit { get; set; }
}

// Approve party booking DTO
public class ApprovePartyBookingDto
{
    [MaxLength(500)]
    public string? AdminNotes { get; set; }
}

public class RejectPartyBookingDto
{
    [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}