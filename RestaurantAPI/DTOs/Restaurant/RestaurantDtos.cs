using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Restaurant;

public class RestaurantInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Dictionary<string, string>? OpeningHours { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public bool RequireDeposit { get; set; }
    public int MinAdvanceHours { get; set; }
    public int CancellationHours { get; set; }
}