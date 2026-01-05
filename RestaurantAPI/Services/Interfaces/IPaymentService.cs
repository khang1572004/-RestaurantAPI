using RestaurantAPI.DTOs.Payment;

namespace RestaurantAPI.Services.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponse> CreateDepositPaymentAsync(CreateDepositPaymentDto dto, string ipAddress);
    Task<(bool Success, string Message, PaymentDto? Payment)> ProcessVNPayCallbackAsync(IQueryCollection queryParams);
    Task<(bool Success, string Message, PaymentDto? Payment)> ProcessMoMoCallbackAsync(IQueryCollection queryParams);
    Task<List<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId);
    Task<List<PaymentDto>> GetPaymentsByCustomerIdAsync(int customerId);
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
}