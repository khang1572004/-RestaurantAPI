using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Category;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// CN053: Xem danh sách danh mục
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
    }

    /// <summary>
    /// Xem chi tiết danh mục
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy danh mục"));
        }
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
    }

    /// <summary>
    /// CN045: Thêm danh mục
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _categoryService.CreateCategoryAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<CategoryDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN045: Sửa danh mục
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _categoryService.UpdateCategoryAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<CategoryDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN045: Xóa danh mục
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _categoryService.DeleteCategoryAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }
}