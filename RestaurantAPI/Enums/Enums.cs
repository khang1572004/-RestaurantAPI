public enum UserRole
{
    Customer = 1,
    Staff = 2,
    Admin = 3
}

public enum BookingStatus
{
    Pending = 1,     // Chờ xác nhận
    Confirmed = 2,   // Đã xác nhận
    CheckedIn = 3,   // Đã check-in
    Completed = 4,   // Hoàn thành
    Cancelled = 5,   // Đã hủy
    NoShow = 6       // Không
}

public enum PaymentStatus
{
    Unpaid = 1,      // Chưa thanh toán
    Partial = 2,     // Đã cọc (thanh toán một phần)
    Paid = 3         // Đã thanh toán đủ
}

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Cancelled = 3
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    VNPay = 3,
    MoMo = 4
}

public enum PaymentType
{
    Deposit = 1,    // Đặt cọc
    Full = 2,       // Thanh toán toàn bộ
    Remaining = 3   // Thanh toán phần còn lại
}

public enum PaymentStatusEnum
{
    Pending = 1,
    Success = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}


public enum RefundStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4
}

public enum TableStatus
{
    Available = 1,
    Reserved = 2,    // Đã đặt
    Occupied = 3,    // Đang sử dụng
    Maintenance = 4  // Bảo trì
}

public enum DiscountType
{
    Percentage = 1, // Giảm %
    Fixed = 2       // Giảm cố định
}