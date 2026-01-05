using RestaurantAPI.DTOs.TableType;

namespace RestaurantAPI.Services.Interfaces;

public interface ITableTypeService
{
    Task<List<TableTypeDto>> GetAllTableTypesAsync(bool includeInactive = false);
    Task<TableTypeDto?> GetTableTypeByIdAsync(int id);
    Task<(bool Success, string Message, TableTypeDto? Data)> CreateTableTypeAsync(CreateTableTypeDto dto);
    Task<(bool Success, string Message, TableTypeDto? Data)> UpdateTableTypeAsync(int id, UpdateTableTypeDto dto);
    Task<(bool Success, string Message)> DeleteTableTypeAsync(int id);
}