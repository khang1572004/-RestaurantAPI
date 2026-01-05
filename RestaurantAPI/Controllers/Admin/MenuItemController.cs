using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.MenuItem;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/menu-items")]
[Authorize(Roles = "Admin")]
public class MenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;
    private readonly IFileService _fileService;
    private readonly ILogger<MenuItemController> _logger;

    public MenuItemController(
        IMenuItemService menuItemService,
        IFileService fileService,
        ILogger<MenuItemController> logger)
    {
        _menuItemService = menuItemService;
        _fileService = fileService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// CN053: Xem danh sách món ăn
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] bool includeInactive = false)
    {
        var items = await _menuItemService.GetAllMenuItemsAsync(categoryId, includeInactive);
        return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(items));
    }

    /// <summary>
    /// Xem chi tiết món ăn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _menuItemService.GetMenuItemByIdAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy món ăn"));
        }
        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(item));
    }

    /// <summary>
    /// CN046: Thêm món ăn
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var userId = GetUserId();
        var (success, message, data) = await _menuItemService.CreateMenuItemAsync(dto, userId);

        if (!success)
        {
            return BadRequest(ApiResponse<MenuItemDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN047: Sửa món ăn
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var userId = GetUserId();
        var (success, message, data) = await _menuItemService.UpdateMenuItemAsync(id, dto, userId);

        if (!success)
        {
            return BadRequest(ApiResponse<MenuItemDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN048: Xóa món ăn
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _menuItemService.DeleteMenuItemAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN049: Upload hình ảnh món ăn
    /// </summary>
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("File không hợp lệ"));
        }

        // Upload file
        var (uploadSuccess, uploadMessage, filePath) = await _fileService.UploadFileAsync(file, "menu-items");

        if (!uploadSuccess)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(uploadMessage));
        }

        // Update menu item
        var (success, message) = await _menuItemService.UpdateMenuItemImageAsync(id, filePath!);

        if (!success)
        {
            // Delete uploaded file if update fails
            await _fileService.DeleteFileAsync(filePath!);
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new { ImageUrl = filePath }, message));
    }

    /// <summary>
    /// CN050: Cập nhật giá món
    /// </summary>
    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdateMenuItemPriceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var userId = GetUserId();
        var (success, message) = await _menuItemService.UpdateMenuItemPriceAsync(id, dto, userId);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN051: Đánh dấu món Hết/Tạm ngừng
    /// </summary>
    [HttpPut("{id}/availability")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] UpdateMenuItemAvailabilityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message) = await _menuItemService.UpdateMenuItemAvailabilityAsync(id, dto.IsAvailable);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN052: Sắp xếp thứ tự món ăn
    /// </summary>
    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderMenuItemsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message) = await _menuItemService.ReorderMenuItemsAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }
}