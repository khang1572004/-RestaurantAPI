using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Zone;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ZoneController : ControllerBase
{
    private readonly IZoneService _zoneService;

    public ZoneController(IZoneService zoneService)
    {
        _zoneService = zoneService;
    }

    /// <summary>
    /// CN064: Xem danh sách khu vực
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var zones = await _zoneService.GetAllZonesAsync(includeInactive);
        return Ok(ApiResponse<List<ZoneDto>>.SuccessResponse(zones));
    }

    /// <summary>
    /// Xem chi tiết khu vực
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var zone = await _zoneService.GetZoneByIdAsync(id);
        if (zone == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy khu vực"));
        }
        return Ok(ApiResponse<ZoneDto>.SuccessResponse(zone));
    }

    /// <summary>
    /// CN063: Thêm khu vực
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateZoneDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _zoneService.CreateZoneAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<ZoneDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<ZoneDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN063: Sửa khu vực
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateZoneDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _zoneService.UpdateZoneAsync(id, dto);

        if (!success)
        {
            return BadRequest(ApiResponse<ZoneDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<ZoneDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN063: Xóa khu vực
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _zoneService.DeleteZoneAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }
}