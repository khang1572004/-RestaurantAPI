using RestaurantAPI.DTOs.Voucher;

namespace RestaurantAPI.Services.Interfaces;

public interface IVoucherService
{
    // Existing methods (Phase 3)
    Task<List<VoucherDto>> GetActiveVouchersAsync();
    Task<VoucherValidationResult> ValidateVoucherAsync(string code, int customerId, decimal? orderValue = null);
    Task<decimal> CalculateDiscountAsync(Voucher voucher, decimal orderValue);
    Task<bool> CanCustomerUseVoucherAsync(int voucherId, int customerId);
    Task RecordVoucherUsageAsync(int voucherId, int customerId, int? bookingId = null, int? orderId = null);

    // NEW - CRUD Operations (CN076-CN080)
    Task<(List<VoucherDto> Vouchers, int TotalCount)> GetAllVouchersAsync(VoucherFilterDto filter);
    Task<VoucherDto?> GetVoucherByIdAsync(int id);
    Task<(bool Success, string Message, VoucherDto? Data)> CreateVoucherAsync(CreateVoucherDto dto);
    Task<(bool Success, string Message, VoucherDto? Data)> UpdateVoucherAsync(int id, UpdateVoucherDto dto);
    Task<(bool Success, string Message)> DeleteVoucherAsync(int id);
    Task<(bool Success, string Message)> ToggleVoucherAsync(int id, bool isActive);

    // NEW - Statistics (CN081)
    Task<VoucherStatsDto> GetVoucherStatsAsync(int voucherId);
}