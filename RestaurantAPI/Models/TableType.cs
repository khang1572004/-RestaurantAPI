using System.ComponentModel.DataAnnotations;

public class TableType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal DepositAmount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; } 

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Table> Tables { get; set; } = new List<Table>();
}