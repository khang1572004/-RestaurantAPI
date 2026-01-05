using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.TableType;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/table-types")]
[Authorize(Roles = "Admin")]
public class TableTypeController : ControllerBase
{
    private readonly ITableTypeService _tableTypeService;

    public TableTypeController(ITableTypeService tableTypeService)
    {
        _tableTypeService = tableTypeService;
    }

    /// <summary>
    /// CN056: Xem danh sách loại bàn
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var types = await _tableTypeService.GetAllTableTypesAsync(includeInactive);
        return Ok(ApiResponse<List<TableTypeDto>>.SuccessResponse(types));
    }

    /// <summary>
    /// Xem chi tiết loại bàn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var type = await _tableTypeService.GetTableTypeByIdAsync(id);
        if (type == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy loại bàn"));
        }
        return Ok(ApiResponse<TableTypeDto>.SuccessResponse(type));
    }

    /// <summary>
    /// CN054: Thêm loại bàn
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTableTypeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _tableTypeService.CreateTableTypeAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<TableTypeDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<TableTypeDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN054: Sửa loại bàn
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTableTypeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _tableTypeService.UpdateTableTypeAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<TableTypeDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<TableTypeDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN054: Xóa loại bàn
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _tableTypeService.DeleteTableTypeAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }
}