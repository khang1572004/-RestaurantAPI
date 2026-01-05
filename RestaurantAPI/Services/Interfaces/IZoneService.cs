using RestaurantAPI.DTOs.Zone;

namespace RestaurantAPI.Services.Interfaces;

public interface IZoneService
{
    Task<List<ZoneDto>> GetAllZonesAsync(bool includeInactive = false);
    Task<ZoneDto?> GetZoneByIdAsync(int id);
    Task<(bool Success, string Message, ZoneDto? Data)> CreateZoneAsync(CreateZoneDto dto);
    Task<(bool Success, string Message, ZoneDto? Data)> UpdateZoneAsync(int id, UpdateZoneDto dto);
    Task<(bool Success, string Message)> DeleteZoneAsync(int id);
}