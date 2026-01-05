using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.TableType;

public class TableTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DepositAmount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TablesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}