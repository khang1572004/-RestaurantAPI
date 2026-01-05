using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs.Table;

public class TableDto
{
    public int Id { get; set; }
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public int TableTypeId { get; set; }
    public string TableTypeName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal? CustomDepositAmount { get; set; }
    public decimal EffectiveDepositAmount { get; set; } // Deposit amount to actually use
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public string? MaintenanceNote { get; set; }
}