using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class UpdateTableDto
{
    public int? ZoneId { get; set; }

    [Required(ErrorMessage = "Loại bàn là bắt buộc")]
    public int TableTypeId { get; set; }

    [Required(ErrorMessage = "Số bàn là bắt buộc")]
    [MaxLength(20)]
    public string TableNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sức chứa là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Sức chứa phải từ 1-50 người")]
    public int Capacity { get; set; }

    [Range(0, 10000000, ErrorMessage = "Tiền cọc phải từ 0 đến 10,000,000")]
    public decimal? CustomDepositAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastMaintenanceDate { get; set; }

    [MaxLength(500)]
    public string? MaintenanceNote { get; set; }
}