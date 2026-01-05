using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.DTOs.Table;

namespace RestaurantAPI.Services.Interfaces;

public interface ITableService
{
    Task<List<TableDto>> GetAllTablesAsync(int? zoneId = null, int? tableTypeId = null, TableStatus? status = null);
    Task<TableDto?> GetTableByIdAsync(int id);
    Task<(bool Success, string Message, TableDto? Data)> CreateTableAsync(CreateTableDto dto);
    Task<(bool Success, string Message, List<TableDto>? Data)> BulkCreateTablesAsync(BulkCreateTablesDto dto);
    Task<(bool Success, string Message, TableDto? Data)> UpdateTableAsync(int id, UpdateTableDto dto);
    Task<(bool Success, string Message)> DeleteTableAsync(int id);
    Task<(bool Success, string Message)> UpdateTableStatusAsync(int id, UpdateTableStatusDto dto);
}