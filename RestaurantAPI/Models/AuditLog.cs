using System.ComponentModel.DataAnnotations;

public class AuditLog
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public int? RecordId { get; set; }

    public string? OldValue { get; set; } 
    public string? NewValue { get; set; } 

    public int? UserId { get; set; }

    [MaxLength(100)]
    public string? IpAddress { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}