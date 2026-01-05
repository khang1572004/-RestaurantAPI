using System.ComponentModel.DataAnnotations;

public class EmailLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }

    [Required, EmailAddress, MaxLength(100)]
    public string EmailTo { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string EmailType { get; set; } = string.Empty;

    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; } 

    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}