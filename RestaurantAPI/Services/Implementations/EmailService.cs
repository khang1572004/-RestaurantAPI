using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RestaurantAPI.Helpers;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly ApplicationDbContext _context;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            var enableNotifications = _configuration.GetValue<bool>("SystemSettings:EnableEmailNotifications");
            
            if (!enableNotifications)
            {
                _logger.LogInformation("Email notifications disabled. Email to {Email} not sent", toEmail);
                return true; // Return true để không block flow trong development
            }

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var enableSsl = _configuration.GetValue<bool>("Email:EnableSsl");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
            {
                builder.HtmlBody = body;
            }
            else
            {
                builder.TextBody = body;
            }
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            // Log email sent
            await LogEmailAsync(toEmail, subject, body, "General", true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            await LogEmailAsync(toEmail, subject, body, "General", false, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string verificationToken)
    {
        var verificationUrl = $"{_configuration["AppSettings:FrontendUrl"]}/verify-email?token={verificationToken}";
        var subject = "Xác nhận email - Restaurant Booking System";
        var body = EmailTemplates.GetVerificationEmailTemplate(userName, verificationUrl);

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken)
    {
        var resetUrl = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?token={resetToken}";
        var subject = "Đặt lại mật khẩu - Restaurant Booking System";
        var body = EmailTemplates.GetPasswordResetEmailTemplate(userName, resetUrl);

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendBookingConfirmationEmailAsync(
        string toEmail,
        string userName,
        string bookingCode,
        DateTime bookingDate,
        TimeSpan bookingTime)
    {
        var subject = $"Xác nhận đặt bàn #{bookingCode}";
        var body = EmailTemplates.GetBookingConfirmationTemplate(userName, bookingCode, bookingDate, bookingTime);

        return await SendEmailAsync(toEmail, subject, body);
    }

    private async Task LogEmailAsync(string toEmail, string subject, string body, string emailType, bool isSent, string? errorMessage = null)
    {
        try
        {
            var emailLog = new EmailLog
            {
                EmailTo = toEmail,
                Subject = subject,
                Body = body,
                EmailType = emailType,
                IsSent = isSent,
                SentAt = isSent ? DateTime.UtcNow : null,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailLogs.Add(emailLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging email");
        }
    }
}