namespace RestaurantAPI.Services.Interfaces;

public interface IVNPayService
{
    string CreatePaymentUrl(int paymentId, decimal amount, string orderInfo, string returnUrl, string ipAddress);
    bool ValidateCallback(IQueryCollection queryParams, out string transactionId, out decimal amount);
    Task<(bool Success, string Message)> ProcessRefundAsync(string transactionId, decimal amount, string reason);
}