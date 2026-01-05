using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public decimal Subtotal { get; set; }

    // Navigation - ✅ Thêm null!
    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}