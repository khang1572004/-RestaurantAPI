using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Auth;
using RestaurantAPI.DTOs.User;
using RestaurantAPI.Helpers;
using System.Security.Claims;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin user hiện tại
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Không tìm thấy thông tin người dùng"));
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Người dùng không tồn tại"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// CN004: Cập nhật thông tin cá nhân
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                ));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Không tìm thấy thông tin người dùng"));
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Người dùng không tồn tại"));
            }

            // Check if phone number is already used by another user
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.Phone == dto.Phone && u.Id != userId);

                if (phoneExists)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Số điện thoại đã được sử dụng"));
                }
            }

            // Update user info
            user.FullName = dto.FullName;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            };

            _logger.LogInformation("User {UserId} updated profile successfully", userId);
            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "Cập nhật thông tin thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi khi cập nhật thông tin"));
        }
    }
}