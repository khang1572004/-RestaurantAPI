using System.ComponentModel.DataAnnotations;

public class Table
{
    public int Id { get; set; }
    public int? ZoneId { get; set; }
    public int TableTypeId { get; set; }

    [Required, MaxLength(20)]
    public string TableNumber { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    public decimal? CustomDepositAmount { get; set; }

    [Required]
    public TableStatus Status { get; set; } = TableStatus.Available;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMaintenanceDate { get; set; }

    [MaxLength(500)]
    public string? MaintenanceNote { get; set; } 

    // Navigation
    public Zone? Zone { get; set; }
    public TableType TableType { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}


