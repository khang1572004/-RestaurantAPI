using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs.Auth;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// CN001: Đăng ký tài khoản
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _authService.RegisterAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN002: Đăng nhập
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message, data) = await _authService.LoginAsync(dto);

        if (!success)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// CN003: Quên mật khẩu
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message) = await _authService.ForgotPasswordAsync(dto);

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// Đặt lại mật khẩu
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        var (success, message) = await _authService.ResetPasswordAsync(dto);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// Xác nhận email
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Token không hợp lệ"));
        }

        var (success, message) = await _authService.VerifyEmailAsync(token);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    /// <summary>
    /// Làm mới access token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token không hợp lệ"));
        }

        var (success, message, data) = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (!success)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data!, message));
    }
}