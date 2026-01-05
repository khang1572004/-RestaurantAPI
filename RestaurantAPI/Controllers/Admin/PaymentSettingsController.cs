using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Payment;
using RestaurantAPI.Helpers;

namespace RestaurantAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/payment-settings")]
[Authorize(Roles = "Admin")]
public class PaymentSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentSettingsController> _logger;

    public PaymentSettingsController(
        ApplicationDbContext context,
        ILogger<PaymentSettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// CN082: Xem cài đặt thanh toán
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _context.PaymentSettings
            .Select(s => new PaymentSettingsDto
            {
                Provider = s.Provider,
                ApiKey = MaskSecret(s.ApiKey),
                SecretKey = MaskSecret(s.SecretKey),
                ReturnUrl = s.ReturnUrl,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PaymentSettingsDto>>.SuccessResponse(settings));
    }

    /// <summary>
    /// CN082: Cập nhật cài đặt VNPay
    /// </summary>
    [HttpPut("vnpay")]
    public async Task<IActionResult> UpdateVNPay([FromBody] UpdatePaymentSettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        try
        {
            var setting = await _context.PaymentSettings
                .FirstOrDefaultAsync(s => s.Provider == "VNPay");

            if (setting == null)
            {
                setting = new PaymentSetting
                {
                    Provider = "VNPay",
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentSettings.Add(setting);
            }

            setting.ApiKey = dto.ApiKey;
            setting.SecretKey = dto.SecretKey;
            setting.ReturnUrl = dto.ReturnUrl;
            setting.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("VNPay payment settings updated");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật cài đặt VNPay thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating VNPay settings");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi"));
        }
    }

    /// <summary>
    /// CN082: Cập nhật cài đặt MoMo
    /// </summary>
    [HttpPut("momo")]
    public async Task<IActionResult> UpdateMoMo([FromBody] UpdatePaymentSettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            ));
        }

        try
        {
            var setting = await _context.PaymentSettings
                .FirstOrDefaultAsync(s => s.Provider == "MoMo");

            if (setting == null)
            {
                setting = new PaymentSetting
                {
                    Provider = "MoMo",
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentSettings.Add(setting);
            }

            setting.ApiKey = dto.ApiKey;
            setting.SecretKey = dto.SecretKey;
            setting.ReturnUrl = dto.ReturnUrl;
            setting.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("MoMo payment settings updated");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật cài đặt MoMo thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating MoMo settings");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Đã xảy ra lỗi"));
        }
    }

    /// <summary>
    /// CN082: Test kết nối thanh toán
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection([FromQuery] string provider)
    {
        try
        {
            var setting = await _context.PaymentSettings
                .FirstOrDefaultAsync(s => s.Provider == provider);

            if (setting == null)
            {
                return NotFound(ApiResponse<TestConnectionResponse>.ErrorResponse(
                    $"Không tìm thấy cài đặt cho {provider}"));
            }

            // Basic validation
            if (string.IsNullOrEmpty(setting.ApiKey) || string.IsNullOrEmpty(setting.SecretKey))
            {
                return BadRequest(ApiResponse<TestConnectionResponse>.ErrorResponse(
                    "Thiếu thông tin API Key hoặc Secret Key"));
            }

            var response = new TestConnectionResponse
            {
                Success = true,
                Message = $"Cấu hình {provider} hợp lệ",
                Details = $"API Key: {MaskSecret(setting.ApiKey)}, Active: {setting.IsActive}"
            };

            return Ok(ApiResponse<TestConnectionResponse>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection for {Provider}", provider);
            return StatusCode(500, ApiResponse<TestConnectionResponse>.ErrorResponse("Đã xảy ra lỗi"));
        }
    }

    private static string MaskSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret) || secret.Length <= 8)
            return "****";

        return secret.Substring(0, 4) + "****" + secret.Substring(secret.Length - 4);
    }
}