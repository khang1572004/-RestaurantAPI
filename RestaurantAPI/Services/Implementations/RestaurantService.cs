using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.Services.Interfaces;
using System.Text.Json;

namespace RestaurantAPI.Services.Implementations;

public class RestaurantService : IRestaurantService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(ApplicationDbContext context, ILogger<RestaurantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RestaurantInfoDto> GetRestaurantInfoAsync()
    {
        var settings = await _context.RestaurantSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

        var info = new RestaurantInfoDto
        {
            Name = settings.GetValueOrDefault("restaurant_name", "Nhà Hàng"),
            Address = settings.GetValueOrDefault("restaurant_address", ""),
            Phone = settings.GetValueOrDefault("restaurant_phone", ""),
            Email = settings.GetValueOrDefault("restaurant_email", ""),
            Logo = settings.GetValueOrDefault("restaurant_logo"),
            CoverImage = settings.GetValueOrDefault("restaurant_cover"),
            RequireDeposit = bool.Parse(settings.GetValueOrDefault("require_deposit", "true")),
            MinAdvanceHours = int.Parse(settings.GetValueOrDefault("min_advance_hours", "2")),
            CancellationHours = int.Parse(settings.GetValueOrDefault("cancellation_hours", "4"))
        };

        // Parse opening hours JSON
        var openingHoursJson = settings.GetValueOrDefault("opening_hours");
        if (!string.IsNullOrEmpty(openingHoursJson))
        {
            try
            {
                info.OpeningHours = JsonSerializer.Deserialize<Dictionary<string, string>>(openingHoursJson);
            }
            catch
            {
                info.OpeningHours = new Dictionary<string, string>();
            }
        }

        return info;
    }

    public async Task<bool> UpdateRestaurantSettingsAsync(UpdateRestaurantSettingsDto dto)
    {
        try
        {
            await UpdateSettingAsync("restaurant_name", dto.Name);
            await UpdateSettingAsync("restaurant_address", dto.Address);
            await UpdateSettingAsync("restaurant_phone", dto.Phone);
            await UpdateSettingAsync("restaurant_email", dto.Email);

            if (!string.IsNullOrEmpty(dto.Logo))
                await UpdateSettingAsync("restaurant_logo", dto.Logo);

            if (!string.IsNullOrEmpty(dto.CoverImage))
                await UpdateSettingAsync("restaurant_cover", dto.CoverImage);

            _logger.LogInformation("Restaurant settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant settings");
            return false;
        }
    }

    public async Task<bool> UpdateOpeningHoursAsync(UpdateOpeningHoursDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto.OpeningHours);
            await UpdateSettingAsync("opening_hours", json);

            _logger.LogInformation("Opening hours updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opening hours");
            return false;
        }
    }

    public async Task<bool> UpdateBookingPolicyAsync(UpdateBookingPolicyDto dto)
    {
        try
        {
            await UpdateSettingAsync("min_advance_hours", dto.MinAdvanceHours.ToString());
            await UpdateSettingAsync("cancellation_hours", dto.CancellationHours.ToString());
            await UpdateSettingAsync("require_deposit", dto.RequireDeposit.ToString().ToLower());

            if (dto.DefaultDepositAmount.HasValue)
                await UpdateSettingAsync("default_deposit_amount", dto.DefaultDepositAmount.Value.ToString());

            _logger.LogInformation("Booking policy updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking policy");
            return false;
        }
    }

    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _context.RestaurantSettings
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
        var setting = await _context.RestaurantSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new RestaurantSetting
            {
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            };
            _context.RestaurantSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}