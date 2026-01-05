using System.ComponentModel.DataAnnotations;

public class PaymentSetting
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ReturnUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}