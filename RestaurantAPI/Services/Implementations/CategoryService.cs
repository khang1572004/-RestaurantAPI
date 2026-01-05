using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Category;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        var query = _context.Categories
            .Include(c => c.MenuItems)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Position = c.Position,
                IsActive = c.IsActive,
                MenuItemsCount = c.MenuItems.Count(m => m.IsAvailable),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Position = category.Position,
            IsActive = category.IsActive,
            MenuItemsCount = category.MenuItems.Count(m => m.IsAvailable),
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            // Check duplicate name
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
            {
                return (false, "Tên danh mục đã tồn tại", null);
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Position = dto.Position,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Position = category.Position,
                IsActive = category.IsActive,
                MenuItemsCount = 0,
                CreatedAt = category.CreatedAt
            };

            _logger.LogInformation("Category created: {CategoryName}", category.Name);
            return (true, "Tạo danh mục thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return (false, "Đã xảy ra lỗi khi tạo danh mục", null);
        }
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return (false, "Không tìm thấy danh mục", null);
            }

            // Check duplicate name (exclude current)
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            {
                return (false, "Tên danh mục đã tồn tại", null);
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Position = dto.Position;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var result = await GetCategoryByIdAsync(id);

            _logger.LogInformation("Category updated: {CategoryId}", id);
            return (true, "Cập nhật danh mục thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật danh mục", null);
        }
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return (false, "Không tìm thấy danh mục");
            }

            // Check if category has menu items
            if (category.MenuItems.Any())
            {
                return (false, "Không thể xóa danh mục có món ăn. Vui lòng xóa các món ăn trước.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category deleted: {CategoryId}", id);
            return (true, "Xóa danh mục thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return (false, "Đã xảy ra lỗi khi xóa danh mục");
        }
    }
}