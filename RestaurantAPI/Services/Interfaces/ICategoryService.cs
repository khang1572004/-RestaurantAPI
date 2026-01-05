using RestaurantAPI.DTOs.Category;

namespace RestaurantAPI.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<(bool Success, string Message, CategoryDto? Data)> CreateCategoryAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message, CategoryDto? Data)> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
}