using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.MenuItem;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class MenuItemService : IMenuItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MenuItemService> _logger;

    public MenuItemService(ApplicationDbContext context, ILogger<MenuItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MenuItemDto>> GetAllMenuItemsAsync(int? categoryId = null, bool includeInactive = false)
    {
        var query = _context.MenuItems
            .Include(m => m.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(m => m.IsAvailable);
        }

        var items = await query
            .OrderBy(m => m.Position)
            .ThenBy(m => m.Name)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                ImageUrl = m.ImageUrl,
                IsAvailable = m.IsAvailable,
                Position = m.Position,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .ToListAsync();

        return items;
    }

    public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
            return null;

        return new MenuItemDto
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            Position = item.Position,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    public async Task<List<MenuItemDto>> SearchMenuItemsAsync(string query)
    {
        var items = await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.IsAvailable && m.Name.Contains(query))
            .OrderBy(m => m.Position)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                ImageUrl = m.ImageUrl,
                IsAvailable = m.IsAvailable,
                Position = m.Position,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .ToListAsync();

        return items;
    }

    public async Task<(bool Success, string Message, MenuItemDto? Data)> CreateMenuItemAsync(CreateMenuItemDto dto, int userId)
    {
        try
        {
            // Check category exists
            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            {
                return (false, "Danh mục không tồn tại", null);
            }

            var menuItem = new MenuItem
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Position = dto.Position,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var result = await GetMenuItemByIdAsync(menuItem.Id);

            _logger.LogInformation("Menu item created: {MenuItemName} by user {UserId}", menuItem.Name, userId);
            return (true, "Tạo món ăn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return (false, "Đã xảy ra lỗi khi tạo món ăn", null);
        }
    }

    public async Task<(bool Success, string Message, MenuItemDto? Data)> UpdateMenuItemAsync(int id, UpdateMenuItemDto dto, int userId)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return (false, "Không tìm thấy món ăn", null);
            }

            // Check category exists
            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            {
                return (false, "Danh mục không tồn tại", null);
            }

            // Track price change
            if (menuItem.Price != dto.Price)
            {
                var priceHistory = new MenuItemPriceHistory
                {
                    MenuItemId = id,
                    OldPrice = menuItem.Price,
                    NewPrice = dto.Price,
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = "Cập nhật món ăn"
                };
                _context.MenuItemPriceHistories.Add(priceHistory);
            }

            menuItem.CategoryId = dto.CategoryId;
            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description;
            menuItem.Price = dto.Price;
            menuItem.IsAvailable = dto.IsAvailable;
            menuItem.Position = dto.Position;
            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            var result = await GetMenuItemByIdAsync(id);

            _logger.LogInformation("Menu item updated: {MenuItemId} by user {UserId}", id, userId);
            return (true, "Cập nhật món ăn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item {MenuItemId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật món ăn", null);
        }
    }

    public async Task<(bool Success, string Message)> DeleteMenuItemAsync(int id)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return (false, "Không tìm thấy món ăn");
            }

            // Soft delete
            menuItem.IsAvailable = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Menu item deleted: {MenuItemId}", id);
            return (true, "Xóa món ăn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item {MenuItemId}", id);
            return (false, "Đã xảy ra lỗi khi xóa món ăn");
        }
    }

    public async Task<(bool Success, string Message)> UpdateMenuItemImageAsync(int id, string imageUrl)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return (false, "Không tìm thấy món ăn");
            }

            menuItem.ImageUrl = imageUrl;
            menuItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Menu item image updated: {MenuItemId}", id);
            return (true, "Cập nhật hình ảnh thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item image {MenuItemId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật hình ảnh");
        }
    }

    public async Task<(bool Success, string Message)> UpdateMenuItemPriceAsync(int id, UpdateMenuItemPriceDto dto, int userId)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return (false, "Không tìm thấy món ăn");
            }

            var oldPrice = menuItem.Price;

            // Log price history
            var priceHistory = new MenuItemPriceHistory
            {
                MenuItemId = id,
                OldPrice = oldPrice,
                NewPrice = dto.NewPrice,
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow,
                Reason = dto.Reason
            };

            _context.MenuItemPriceHistories.Add(priceHistory);

            menuItem.Price = dto.NewPrice;
            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Menu item price updated: {MenuItemId} from {OldPrice} to {NewPrice}", 
                id, oldPrice, dto.NewPrice);
            return (true, "Cập nhật giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item price {MenuItemId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật giá");
        }
    }

    public async Task<(bool Success, string Message)> UpdateMenuItemAvailabilityAsync(int id, bool isAvailable)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return (false, "Không tìm thấy món ăn");
            }

            menuItem.IsAvailable = isAvailable;
            menuItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var status = isAvailable ? "Có sẵn" : "Hết món";
            _logger.LogInformation("Menu item availability updated: {MenuItemId} -> {Status}", id, status);
            return (true, $"Cập nhật trạng thái thành công: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item availability {MenuItemId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật trạng thái");
        }
    }

    public async Task<(bool Success, string Message)> ReorderMenuItemsAsync(ReorderMenuItemsDto dto)
    {
        try
        {
            foreach (var item in dto.Items)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.Id);
                if (menuItem != null)
                {
                    menuItem.Position = item.Position;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Menu items reordered");
            return (true, "Sắp xếp món ăn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering menu items");
            return (false, "Đã xảy ra lỗi khi sắp xếp món ăn");
        }
    }
}