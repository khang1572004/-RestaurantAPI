using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Auth;
using RestaurantAPI.Services.Interfaces;
using BCrypt.Net;

namespace RestaurantAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context, 
        ITokenService tokenService, 
        IEmailService emailService, 
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check Email đã tồn tại
            if(await _context.Users.AnyAsync(u =>u.Email == dto.Email))
            {
                return (false, "Email đã được sử dụng", null);
            }
            // Check Phnone đã tồn tại
            if (!string.IsNullOrEmpty(dto.Phone) && await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
            {
                return (false, "Số điện thoại đã được sử dụng", null);
            }

            // Tạo User mới
            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer, // Default role
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email xác thực
            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, user.EmailVerificationToken);

            // Tao token
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

             var response = new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiration(),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Role = user.Role.ToString(),
                    IsEmailVerified = user.IsEmailVerified
                }
            };

            _logger.LogInformation("User registered successfully: {Email}", user.Email);
            return (true, "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.", response);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error during registration");
            return (false, "Đã xảy ra lỗi trong quá trình đăng ký", null);
        }
        
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        try
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return (false, "Email hoặc mật khẩu không đúng", null);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return (false, "Tài khoản đã bị khóa", null);
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiration(),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Role = user.Role.ToString(),
                    IsEmailVerified = user.IsEmailVerified
                }
            };

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);
            return (true, "Đăng nhập thành công", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return (false, "Đã xảy ra lỗi trong quá trình đăng nhập", null);
        }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                // Don't reveal if email exists or not (security)
                return (true, "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu");
            }

            // Generate reset token
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

            await _context.SaveChangesAsync();

            // Send reset email
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, user.PasswordResetToken);

            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
            return (true, "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return (false, "Đã xảy ra lỗi trong quá trình xử lý");
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null)
            {
                return (false, "Token không hợp lệ");
            }

            // Check token expiry
            if (user.PasswordResetExpiry == null || user.PasswordResetExpiry < DateTime.UtcNow)
            {
                return (false, "Token đã hết hạn");
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for: {Email}", user.Email);
            return (true, "Đặt lại mật khẩu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return (false, "Đã xảy ra lỗi trong quá trình đặt lại mật khẩu");
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string token)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                return (false, "Token xác nhận không hợp lệ");
            }

            if (user.IsEmailVerified)
            {
                return (true, "Email đã được xác nhận trước đó");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified successfully for: {Email}", user.Email);
            return (true, "Xác nhận email thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return (false, "Đã xảy ra lỗi trong quá trình xác nhận email");
        }
    }


    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Note: In production, you should store refresh tokens in database
            // For now, we'll just generate new tokens
            // This is a simplified implementation

            // TODO: Implement proper refresh token validation with database storage
            return (false, "Refresh token feature chưa được triển khai đầy đủ", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return (false, "Đã xảy ra lỗi trong quá trình làm mới token", null);
        }
    }
}