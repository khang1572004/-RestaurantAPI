using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class UpdateTableStatusDto
{
    [Required]
    public TableStatus Status { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}