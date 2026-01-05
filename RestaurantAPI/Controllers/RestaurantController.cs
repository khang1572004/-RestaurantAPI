using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    /// <summary>
    /// CN005: Xem thông tin nhà hàng (Public)
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetInfo()
    {
        var info = await _restaurantService.GetRestaurantInfoAsync();
        return Ok(ApiResponse<RestaurantInfoDto>.SuccessResponse(info));
    }
}