using System.ComponentModel.DataAnnotations;

public class MenuItemPriceHistory
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }

    [Required]
    public decimal OldPrice { get; set; }

    [Required]
    public decimal NewPrice { get; set; }

    public int ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Reason { get; set; } 

    // Navigation
    public MenuItem MenuItem { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}