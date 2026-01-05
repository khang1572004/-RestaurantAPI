using System.ComponentModel.DataAnnotations;

public class MenuItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; } 

    [Required]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; } 

    public bool IsAvailable { get; set; } = true;
    public int Position { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<MenuItemPriceHistory> PriceHistories { get; set; } = new List<MenuItemPriceHistory>();
}