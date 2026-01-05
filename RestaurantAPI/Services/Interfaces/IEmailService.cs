namespace RestaurantAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string verificationToken);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken);
    Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, string bookingCode, DateTime bookingDate, TimeSpan bookingTime);
}