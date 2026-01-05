using RestaurantAPI.DTOs.Auth;

namespace RestaurantAPI.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    Task<(bool Success, string Message)> VerifyEmailAsync(string token);
    Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(string refreshToken);
}