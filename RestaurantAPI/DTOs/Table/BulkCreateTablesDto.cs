using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class BulkCreateTablesDto
{
    [Required(ErrorMessage = "Tên khu vực là bắt buộc")]
    [MaxLength(100)]
    public string ZoneName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại bàn là bắt buộc")]
    public int TableTypeId { get; set; }

    [Required(ErrorMessage = "Số lượng bàn là bắt buộc")]
    [Range(1, 100, ErrorMessage = "Số lượng bàn phải từ 1-100")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Tiền tố số bàn là bắt buộc (VD: A, B, C)")]
    [MaxLength(5)]
    public string TablePrefix { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sức chứa là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Sức chứa phải từ 1-50 người")]
    public int Capacity { get; set; }
}