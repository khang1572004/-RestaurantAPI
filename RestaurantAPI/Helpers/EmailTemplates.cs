namespace RestaurantAPI.Helpers;

public static class EmailTemplates
{
    public static string GetVerificationEmailTemplate(string userName, string verificationUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍽️ Restaurant Booking System</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName}!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại Restaurant Booking System.</p>
            <p>Vui lòng nhấn vào nút bên dưới để xác nhận email của bạn:</p>
            <div style='text-align: center;'>
                <a href='{verificationUrl}' class='button'>Xác nhận Email</a>
            </div>
            <p>Hoặc copy link sau vào trình duyệt:</p>
            <p style='background: #fff; padding: 10px; border-radius: 5px; word-break: break-all;'>{verificationUrl}</p>
            <p><strong>Lưu ý:</strong> Link xác nhận này sẽ hết hạn sau 24 giờ.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Restaurant Booking System. All rights reserved.</p>
            <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetPasswordResetEmailTemplate(string userName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #f5576c; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName}!</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Đặt lại mật khẩu</a>
            </div>
            <p>Hoặc copy link sau vào trình duyệt:</p>
            <p style='background: #fff; padding: 10px; border-radius: 5px; word-break: break-all;'>{resetUrl}</p>
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul>
                    <li>Link này sẽ hết hạn sau 1 giờ</li>
                    <li>Chỉ sử dụng link này một lần</li>
                    <li>Không chia sẻ link này với bất kỳ ai</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>© 2024 Restaurant Booking System. All rights reserved.</p>
            <p><strong>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</strong></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetBookingConfirmationTemplate(string userName, string bookingCode, DateTime bookingDate, TimeSpan bookingTime)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .booking-info {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Đặt bàn thành công!</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName}!</h2>
            <p>Cảm ơn bạn đã đặt bàn tại nhà hàng của chúng tôi.</p>
            
            <div class='booking-info'>
                <h3 style='margin-top: 0; color: #4facfe;'>📋 Thông tin đặt bàn</h3>
                <div class='info-row'>
                    <span><strong>Mã đặt bàn:</strong></span>
                    <span style='font-weight: bold; color: #4facfe;'>{bookingCode}</span>
                </div>
                <div class='info-row'>
                    <span><strong>Ngày:</strong></span>
                    <span>{bookingDate:dd/MM/yyyy}</span>
                </div>
                <div class='info-row'>
                    <span><strong>Giờ:</strong></span>
                    <span>{bookingTime:hh\\:mm}</span>
                </div>
            </div>

            <p><strong>Lưu ý:</strong></p>
            <ul>
                <li>Vui lòng đến trước giờ đặt bàn 10-15 phút</li>
                <li>Xuất trình mã đặt bàn khi check-in</li>
                <li>Liên hệ hotline nếu cần hỗ trợ</li>
            </ul>
        </div>
        <div class='footer'>
            <p>© 2024 Restaurant Booking System. All rights reserved.</p>
            <p>Hotline: 1900 xxxx | Email: support@restaurant.com</p>
        </div>
    </div>
</body>
</html>";
    }
}