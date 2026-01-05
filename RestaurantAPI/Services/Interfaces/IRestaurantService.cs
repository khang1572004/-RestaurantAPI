using RestaurantAPI.DTOs.Restaurant;

namespace RestaurantAPI.Services.Interfaces;

public interface IRestaurantService
{
    Task<RestaurantInfoDto> GetRestaurantInfoAsync();
    Task<bool> UpdateRestaurantSettingsAsync(UpdateRestaurantSettingsDto dto);
    Task<bool> UpdateOpeningHoursAsync(UpdateOpeningHoursDto dto);
    Task<bool> UpdateBookingPolicyAsync(UpdateBookingPolicyDto dto);
    Task<string?> GetSettingValueAsync(string key);
    Task<bool> UpdateSettingAsync(string key, string value);
}