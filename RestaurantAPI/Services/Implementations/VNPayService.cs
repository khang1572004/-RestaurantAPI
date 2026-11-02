using RestaurantAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace RestaurantAPI.Services.Implementations;

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VNPayService> _logger;
    private readonly string _vnpayUrl;
    private readonly string _tmnCode;
    private readonly string _hashSecret;

    public VNPayService(IConfiguration configuration, ILogger<VNPayService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // ✅ Validate configuration at startup
        _vnpayUrl = _configuration["VNPay:Url"] 
            ?? throw new InvalidOperationException("VNPay:Url is not configured");
        _tmnCode = _configuration["VNPay:TmnCode"] 
            ?? throw new InvalidOperationException("VNPay:TmnCode is not configured");
        _hashSecret = _configuration["VNPay:HashSecret"] 
            ?? throw new InvalidOperationException("VNPay:HashSecret is not configured");

        _logger.LogInformation("VNPayService initialized with TmnCode: {TmnCode}", _tmnCode);
    }

    public string CreatePaymentUrl(
        int paymentId,
        decimal amount,
        string orderInfo,
        string returnUrl,
        string ipAddress)
    {
        try
        {
            var vnpay = new VNPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", paymentId.ToString());

            var paymentUrl = vnpay.CreateRequestUrl(_vnpayUrl, _hashSecret);

            _logger.LogInformation(
                "VNPay payment URL created for payment {PaymentId}, amount: {Amount}", 
                paymentId, amount);

            return paymentUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment URL for payment {PaymentId}", paymentId);
            throw;
        }
    }

    public bool ValidateCallback(
        IQueryCollection queryParams,
        out string transactionId,
        out decimal amount)
    {
        transactionId = string.Empty;
        amount = 0;

        try
        {
            var vnpay = new VNPayLibrary();

            // ✅ Add all vnp_ parameters to library
            foreach (var param in queryParams)
            {
                if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(param.Key, param.Value!);
                }
            }

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            var responseCode = queryParams["vnp_ResponseCode"].ToString();
            transactionId = queryParams["vnp_TransactionNo"].ToString();
            var amountStr = queryParams["vnp_Amount"].ToString();

            // ✅ Parse amount
            if (long.TryParse(amountStr, out long amountLong))
            {
                amount = amountLong / 100m;
            }
            else
            {
                _logger.LogWarning("Invalid amount in VNPay callback: {Amount}", amountStr);
                return false;
            }

            // ✅ Validate signature
            bool isValidSignature = vnpay.ValidateSignature(vnpSecureHash, _hashSecret);

            if (!isValidSignature)
            {
                _logger.LogWarning("VNPay callback signature validation failed for transaction {TransactionId}", transactionId);
                return false;
            }

            // ✅ Check response code
            if (responseCode != "00")
            {
                _logger.LogWarning(
                    "VNPay payment failed with response code: {ResponseCode} for transaction {TransactionId}", 
                    responseCode, transactionId);
                return false;
            }

            _logger.LogInformation(
                "VNPay callback validated successfully. Transaction: {TransactionId}, Amount: {Amount}", 
                transactionId, amount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating VNPay callback");
            return false;
        }
    }

    public async Task<(bool Success, string Message)> ProcessRefundAsync(
        string transactionId,
        decimal amount,
        string reason)
    {
        try
        {
            // ✅ VNPay refund API implementation
            // Note: VNPay sandbox không hỗ trợ refund, chỉ có production
            // This is a placeholder for production implementation

            _logger.LogInformation(
                "VNPay refund initiated for transaction {TransactionId}, amount: {Amount}, reason: {Reason}", 
                transactionId, amount, reason);
            
            await Task.CompletedTask;
            return (false, "VNPay refund chưa được bật: cần tích hợp API hoàn tiền và callback xác nhận");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay refund for transaction {TransactionId}", transactionId);
            return (false, "Lỗi khi xử lý hoàn tiền VNPay");
        }
    }
}

/// <summary>
/// ✅ FIXED: VNPay Library Helper with corrected signature handling
/// </summary>
public class VNPayLibrary
{
    private readonly SortedList<string, string> _requestData = new();
    private readonly SortedList<string, string> _responseData = new();

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var data = new StringBuilder();
        
        // VNPay signs the URL-encoded query values, then appends the same encoded values.
        // ponytail: keep one canonical query builder so signing and redirect cannot drift.
        var encodedPairs = _requestData
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}")
            .ToList();

        var signString = string.Join("&", encodedPairs);
        var vnpSecureHash = HmacSHA512(hashSecret, signString);
        var queryString = string.Join("&", encodedPairs);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        /*
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(Uri.EscapeDataString(kv.Key));
                data.Append("=");
                data.Append(Uri.EscapeDataString(kv.Value));
                data.Append("&");
            }
        }

        // Remove trailing "&"
        var queryString = data.ToString();
        if (queryString.EndsWith("&"))
        {
            queryString = queryString[..^1];
        }

        // ✅ IMPORTANT: For signature, use the RAW query string (no URL encoding for signature calculation)
        var signData = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                signData.Append(kv.Key);
                signData.Append("=");
                signData.Append(kv.Value);
                signData.Append("&");
            }
        }
        
        var signString = signData.ToString();
        if (signString.EndsWith("&"))
        {
            signString = signString[..^1];
        }

        var vnpSecureHash = HmacSHA512(hashSecret, signString);
        
        return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        */
    }

    public bool ValidateSignature(string inputHash, string hashSecret)
    {
        var data = new StringBuilder();
        
        // VNPay sends decoded query values through ASP.NET. Re-encode them using
        // the same canonical format used when creating the payment URL.
        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value) 
                && kv.Key != "vnp_SecureHash" 
                && kv.Key != "vnp_SecureHashType")
            {
                data.Append(kv.Key);
                data.Append("=");
                data.Append(WebUtility.UrlEncode(kv.Value));
                data.Append("&");
            }
        }

        var signData = data.ToString();
        if (signData.EndsWith("&"))
        {
            signData = signData[..^1];
        }

        var myChecksum = HmacSHA512(hashSecret, signData);
        
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private static string HmacSHA512(string key, string data)
    {
        var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
