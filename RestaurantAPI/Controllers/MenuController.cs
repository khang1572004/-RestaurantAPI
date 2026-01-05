using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Category;
using RestaurantAPI.DTOs.MenuItem;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMenuItemService _menuItemService;

    public MenuController(
        ICategoryService categoryService,
        IMenuItemService menuItemService)
    {
        _categoryService = categoryService;
        _menuItemService = menuItemService;
    }

    /// <summary>
    /// CN007: Xem danh sách danh mục món ăn (Public)
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(includeInactive: false);
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
    }

    /// <summary>
    /// CN006: Xem menu món ăn (Public)
    /// </summary>
    [HttpGet("items")]
    public async Task<IActionResult> GetMenuItems([FromQuery] int? categoryId)
    {
        var items = await _menuItemService.GetAllMenuItemsAsync(categoryId, includeInactive: false);
        return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(items));
    }

    /// <summary>
    /// CN008: Tìm kiếm món ăn (Public)
    /// </summary>
    [HttpGet("items/search")]
    public async Task<IActionResult> SearchMenuItems([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Từ khóa tìm kiếm không hợp lệ"));
        }

        var items = await _menuItemService.SearchMenuItemsAsync(q);
        return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(items));
    }
}