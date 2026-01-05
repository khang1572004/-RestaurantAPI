using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.DTOs.Table;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;
    private readonly ILogger<TableController> _logger;

    public TableController(ITableService tableService, ILogger<TableController> logger)
    {
        _tableService = tableService;
        _logger = logger;
    }

    /// <summary>
    /// CN061: Xem danh sách bàn
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? zoneId,
        [FromQuery] int? tableTypeId,
        [FromQuery] TableStatus? status)
    {
        var tables = await _tableService.GetAllTablesAsync(zoneId, tableTypeId, status);
        return Ok(ApiResponse<List<TableDto>>.SuccessResponse(tables));
    }

    /// <summary>
    /// Xem chi tiết bàn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var table = await _tableService.GetTableByIdAsync(id);
        if (table == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy bàn"));
        }
        return Ok(ApiResponse<TableDto>.SuccessResponse(table));
    }

    /// <summary>
    /// CN058: Thêm bàn thủ công
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _tableService.CreateTableAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<TableDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<TableDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN057: Quick Setup - Tạo bàn hàng loạt
    /// </summary>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateTablesDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _tableService.BulkCreateTablesAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<List<TableDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<TableDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN059: Chỉnh sửa thông tin bàn
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTableDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _tableService.UpdateTableAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<TableDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<TableDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN060: Xóa bàn
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _tableService.DeleteTableAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// CN062: Đặt bàn Reserved/Kích hoạt
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTableStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var (success, message) = await _tableService.UpdateTableStatusAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }
}