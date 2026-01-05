using System.ComponentModel.DataAnnotations;

public class Zone
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; } 

    public int Position { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Table> Tables { get; set; } = new List<Table>();
}