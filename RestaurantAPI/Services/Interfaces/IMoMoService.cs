namespace RestaurantAPI.Services.Interfaces;

public interface IMoMoService
{
    Task<(bool Success, string Message, string? PaymentUrl, string? QrCodeUrl)> CreatePaymentAsync(
        int paymentId, decimal amount, string orderInfo, string returnUrl);
    bool ValidateCallback(IQueryCollection queryParams, out string transactionId, out decimal amount);
    Task<(bool Success, string Message)> ProcessRefundAsync(string transactionId, decimal amount, string reason);
}