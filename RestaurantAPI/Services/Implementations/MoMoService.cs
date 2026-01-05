using RestaurantAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantAPI.Services;

public class MoMoService : IMoMoService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MoMoService> _logger;
    private readonly HttpClient _httpClient;

    public MoMoService(
        IConfiguration configuration,
        ILogger<MoMoService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<(bool Success, string Message, string? PaymentUrl, string? QrCodeUrl)> CreatePaymentAsync(
        int paymentId, decimal amount, string orderInfo, string returnUrl)
    {
        try
        {
            var partnerCode = _configuration["MoMo:PartnerCode"];
            var accessKey = _configuration["MoMo:AccessKey"];
            var secretKey = _configuration["MoMo:SecretKey"];
            var endpoint = _configuration["MoMo:Endpoint"];
            var ipnUrl = _configuration["MoMo:IpnUrl"];

            if (string.IsNullOrEmpty(partnerCode) || string.IsNullOrEmpty(accessKey) || 
                string.IsNullOrEmpty(secretKey))
            {
                return (false, "Chưa cấu hình MoMo credentials trong appsettings.json", null, null);
            }

            var orderId = $"PAYMENT_{paymentId}_{DateTime.Now:yyyyMMddHHmmss}";
            var requestId = Guid.NewGuid().ToString();

            var request = new MoMoPaymentRequest
            {
                PartnerCode = partnerCode,
                AccessKey = accessKey,
                RequestId = requestId,
                Amount = (long)amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                RedirectUrl = returnUrl,
                IpnUrl = ipnUrl,
                RequestType = "captureWallet",
                ExtraData = paymentId.ToString(),
                Lang = "vi"
            };

            // Tạo signature
            var rawSignature = $"accessKey={accessKey}" +
                              $"&amount={request.Amount}" +
                              $"&extraData={request.ExtraData}" +
                              $"&ipnUrl={request.IpnUrl}" +
                              $"&orderId={request.OrderId}" +
                              $"&orderInfo={request.OrderInfo}" +
                              $"&partnerCode={request.PartnerCode}" +
                              $"&redirectUrl={request.RedirectUrl}" +
                              $"&requestId={request.RequestId}" +
                              $"&requestType={request.RequestType}";

            request.Signature = ComputeHmacSha256(rawSignature, secretKey);

            _logger.LogInformation("Creating MoMo payment: OrderId={OrderId}, Amount={Amount}", 
                orderId, amount);

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("MoMo response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MoMoPaymentResponse>(responseContent, 
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            if (momoResponse == null)
            {
                return (false, "Không thể parse response từ MoMo", null, null);
            }

            if (momoResponse.ResultCode == 0)
            {
                return (true, "Tạo payment thành công", momoResponse.PayUrl, momoResponse.QrCodeUrl);
            }

            return (false, momoResponse.Message ?? "Lỗi từ MoMo", null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment");
            return (false, "Có lỗi xảy ra khi tạo thanh toán MoMo", null, null);
        }
    }

    public bool ValidateCallback(IQueryCollection queryParams, out string transactionId, out decimal amount)
    {
        transactionId = string.Empty;
        amount = 0;

        try
        {
            var partnerCode = queryParams["partnerCode"].ToString();
            var orderId = queryParams["orderId"].ToString();
            var requestId = queryParams["requestId"].ToString();
            var amountStr = queryParams["amount"].ToString();
            var orderInfo = queryParams["orderInfo"].ToString();
            var orderType = queryParams["orderType"].ToString();
            transactionId = queryParams["transId"].ToString();
            var resultCode = queryParams["resultCode"].ToString();
            var message = queryParams["message"].ToString();
            var payType = queryParams["payType"].ToString();
            var responseTime = queryParams["responseTime"].ToString();
            var extraData = queryParams["extraData"].ToString();
            var signature = queryParams["signature"].ToString();

            var secretKey = _configuration["MoMo:SecretKey"];

            // Tạo signature để validate
            var rawSignature = $"accessKey={_configuration["MoMo:AccessKey"]}" +
                              $"&amount={amountStr}" +
                              $"&extraData={extraData}" +
                              $"&message={message}" +
                              $"&orderId={orderId}" +
                              $"&orderInfo={orderInfo}" +
                              $"&orderType={orderType}" +
                              $"&partnerCode={partnerCode}" +
                              $"&payType={payType}" +
                              $"&requestId={requestId}" +
                              $"&responseTime={responseTime}" +
                              $"&resultCode={resultCode}" +
                              $"&transId={transactionId}";

            var expectedSignature = ComputeHmacSha256(rawSignature, secretKey);

            if (signature != expectedSignature)
            {
                _logger.LogWarning("Invalid MoMo signature. Expected: {Expected}, Got: {Got}", 
                    expectedSignature, signature);
                return false;
            }

            if (resultCode != "0")
            {
                _logger.LogWarning("MoMo payment failed with resultCode: {ResultCode}, message: {Message}", 
                    resultCode, message);
                return false;
            }

            if (long.TryParse(amountStr, out long amountLong))
            {
                amount = amountLong;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MoMo callback");
            return false;
        }
    }

    public async Task<(bool Success, string Message)> ProcessRefundAsync(
        string transactionId, decimal amount, string reason)
    {
        try
        {
            var partnerCode = _configuration["MoMo:PartnerCode"];
            var accessKey = _configuration["MoMo:AccessKey"];
            var secretKey = _configuration["MoMo:SecretKey"];

            var requestId = Guid.NewGuid().ToString();
            var orderId = $"REFUND_{DateTime.Now:yyyyMMddHHmmss}";

            var refundRequest = new
            {
                partnerCode,
                orderId,
                requestId,
                amount = (long)amount,
                transId = long.Parse(transactionId),
                lang = "vi",
                description = reason
            };

            var rawSignature = $"accessKey={accessKey}" +
                              $"&amount={refundRequest.amount}" +
                              $"&description={refundRequest.description}" +
                              $"&orderId={refundRequest.orderId}" +
                              $"&partnerCode={refundRequest.partnerCode}" +
                              $"&requestId={refundRequest.requestId}" +
                              $"&transId={refundRequest.transId}";

            var signature = ComputeHmacSha256(rawSignature, secretKey);

            var requestWithSignature = new
            {
                refundRequest.partnerCode,
                refundRequest.orderId,
                refundRequest.requestId,
                refundRequest.amount,
                refundRequest.transId,
                refundRequest.lang,
                refundRequest.description,
                signature
            };

            var jsonContent = JsonSerializer.Serialize(requestWithSignature);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://test-payment.momo.vn/v2/gateway/api/refund", 
                httpContent);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("MoMo refund response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MoMoRefundResponse>(responseContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (momoResponse?.ResultCode == 0)
            {
                return (true, "Hoàn tiền thành công");
            }

            return (false, momoResponse?.Message ?? "Lỗi hoàn tiền");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo refund");
            return (false, "Có lỗi xảy ra khi hoàn tiền");
        }
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

#region MoMo Models

public class MoMoPaymentRequest
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    [JsonPropertyName("accessKey")]
    public string AccessKey { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; set; } = string.Empty;

    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; set; } = string.Empty;

    [JsonPropertyName("ipnUrl")]
    public string IpnUrl { get; set; } = string.Empty;

    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "captureWallet";

    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "vi";

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;
}

public class MoMoPaymentResponse
{
    [JsonPropertyName("partnerCode")]
    public string? PartnerCode { get; set; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("payUrl")]
    public string? PayUrl { get; set; }

    [JsonPropertyName("qrCodeUrl")]
    public string? QrCodeUrl { get; set; }

    [JsonPropertyName("deeplink")]
    public string? Deeplink { get; set; }

    [JsonPropertyName("deeplinkWebInApp")]
    public string? DeeplinkWebInApp { get; set; }
}

public class MoMoRefundResponse
{
    [JsonPropertyName("partnerCode")]
    public string? PartnerCode { get; set; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("transId")]
    public long TransId { get; set; }

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }
}

#endregion