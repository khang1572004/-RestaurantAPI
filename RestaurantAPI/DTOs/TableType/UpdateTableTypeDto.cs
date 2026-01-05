using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.TableType;

public class UpdateTableTypeDto
{
    [Required(ErrorMessage = "Tên loại bàn là bắt buộc")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiền cọc là bắt buộc")]
    [Range(0, 10000000, ErrorMessage = "Tiền cọc phải từ 0 đến 10,000,000")]
    public decimal DepositAmount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}