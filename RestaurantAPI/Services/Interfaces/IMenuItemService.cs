using RestaurantAPI.DTOs.MenuItem;

namespace RestaurantAPI.Services.Interfaces;

public interface IMenuItemService
{
    Task<List<MenuItemDto>> GetAllMenuItemsAsync(int? categoryId = null, bool includeInactive = false);
    Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
    Task<List<MenuItemDto>> SearchMenuItemsAsync(string query);
    Task<(bool Success, string Message, MenuItemDto? Data)> CreateMenuItemAsync(CreateMenuItemDto dto, int userId);
    Task<(bool Success, string Message, MenuItemDto? Data)> UpdateMenuItemAsync(int id, UpdateMenuItemDto dto, int userId);
    Task<(bool Success, string Message)> DeleteMenuItemAsync(int id);
    Task<(bool Success, string Message)> UpdateMenuItemImageAsync(int id, string imageUrl);
    Task<(bool Success, string Message)> UpdateMenuItemPriceAsync(int id, UpdateMenuItemPriceDto dto, int userId);
    Task<(bool Success, string Message)> UpdateMenuItemAvailabilityAsync(int id, bool isAvailable);
    Task<(bool Success, string Message)> ReorderMenuItemsAsync(ReorderMenuItemsDto dto);
}