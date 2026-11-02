# Hệ Thống Quản Lý & Đặt Bàn Nhà Hàng (Restaurant Booking & POS System)

Dự án phát triển hệ thống quản lý và đặt bàn nhà hàng chuyên nghiệp, kết hợp giữa Backend dịch vụ API chuẩn RESTful trên nền tảng **ASP.NET Core 9 (C#)** kết hợp cơ sở dữ liệu **PostgreSQL**, cùng giao diện Frontend hiện đại **React 19 + TypeScript + Vite + Tailwind CSS v4** chuẩn thiết kế **UI/UX Pro Max**.

---

## Kiến Trúc Hệ Thống

```
-RestaurantAPI/
├── RestaurantAPI/                 # Backend RESTful Web API (.NET 9 + C# + PostgreSQL)
│   ├── Controllers/               # API Endpoints (Admin, Staff, Customer, Auth, Booking, Menu, POS...)
│   ├── Data/                      # ApplicationDbContext (EF Core Npgsql, Seed data)
│   ├── Models/                    # Thực thể dữ liệu (User, Table, Booking, MenuItem, Order...)
│   ├── Services/                  # Business Logic Services
│   └── appsettings.json           # Cấu hình kết nối DB & JWT
│
├── restaurant-admin-ui/           # Frontend Web Application (React 19 + TS + Vite + Tailwind v4)
│   ├── src/
│   │   ├── components/            # Reusable UI & Layout Components (Sidebar, Header, CustomerLayout)
│   │   ├── pages/                 # Giao diện tính năng (Customer, Admin, Staff, POS, Auth)
│   │   ├── services/              # API Client & Axios Services
│   │   ├── store/                 # Quản lý State toàn cục bằng Zustand
│   │   └── types/                 # TypeScript DTOs & Models
│   └── README.md                  # Hướng dẫn chi tiết UI
│
├── screenshots/                   # Kết quả chụp màn hình kiểm thử tự động (Chrome DevTools)
├── docker-compose.yml             # Cấu hình container PostgreSQL 16 Alpine
└── kilo.json                      # Cấu hình MCP & công cụ phát triển
```

---

## 1. Khởi Động Nhanh Hệ Thống

### Bước 1: Khởi động Cơ sở Dữ liệu PostgreSQL
Yêu cầu đã cài đặt Docker Desktop:
```bash
docker compose up -d postgres
```
*Database `restaurant_booking_dev` sẽ sẵn sàng trên cổng `5432` với user/password là `postgres/postgres`.*

### Bước 2: Khởi động Backend API (.NET 9)
```bash
cd RestaurantAPI
dotnet run
```
- API Endpoint: `http://localhost:5058`
- Swagger UI Documentation: `http://localhost:5058/swagger`

### Bước 3: Khởi động Giao Diện Frontend (React 19)
```bash
cd restaurant-admin-ui
npm install
npm run dev
```
- Truy cập trình duyệt: `http://localhost:5173`

---

## 2. Các Phân Hệ Tính Năng

### 🌟 Cổng Khách Hàng (Customer Portal)
- **Trang chủ (`/`)**: Khám phá ẩm thực tinh hoa Fine Dining, thực đơn nổi bật, đánh giá thực khách.
- **Thực đơn (`/menu`)**: Danh mục món ăn chi tiết, giá niêm yết VND, tình trạng sẵn có.
- **Đặt bàn trực tuyến 3 bước (`/book-table`)**: Chọn ngày giờ -> Hệ thống tìm bàn trống thời gian thực theo khu vực -> Đặt bàn giữ chỗ tức thì.
- **Lịch sử đặt bàn (`/my-bookings`)**: Quản lý các lượt đặt bàn cá nhân, trạng thái, hủy bàn kèm lý do hoặc gửi đánh giá.

### 💼 Cổng Nhân Viên & Điểm Bán (Staff & POS Portal)
- **Sơ đồ bàn trực quan (`/tables`)**: Màu sắc trực quan theo 4 trạng thái (Trống, Đã đặt, Đang dùng bữa, Cần dọn), lọc theo khu vực (Khu A, Khu B, Sân vườn, VIP), check-in khách đến hoặc khách vãng lai.
- **Màn hình Điểm Bán POS (`/pos`)**: Gọi món nhanh, thêm ghi chú bếp, áp dụng voucher giảm giá, cấn trừ tiền cọc bàn, thanh toán nhiều phương thức (Tiền mặt, Thẻ ngân hàng, QR Code).
- **Quản lý đặt bàn (`/bookings`)**: Danh sách khách đặt trong ngày, check-in, hủy đặt bàn.
- **Ca trực nhân viên (`/shifts`)**: Theo dõi doanh thu theo ca, giao ca an toàn.

### 🛡️ Cổng Quản Trị Hệ Thống (Admin Portal)
- **Quản lý Nhân sự (`/admin/staff`)**: Thêm, phân quyền nhân viên (Admin, Staff, Manager), quản lý trạng thái tài khoản.
- **Xử lý Hoàn tiền (`/admin/refunds`)**: Tiếp nhận và xét duyệt các yêu cầu hoàn cọc từ việc hủy đặt bàn.
- **Kiểm duyệt Đánh giá (`/admin/reviews`)**: Quản lý và kiểm duyệt các phản hồi từ khách hàng.
- **Quản lý Thực đơn & Bàn (`/admin/menu`, `/admin/tables-management`)**: Tùy chỉnh danh mục, món ăn, cập nhật giá, thêm bàn và sơ đồ khu vực.
- **Báo cáo & Thống kê (`/admin/reports`)**: Biểu đồ doanh thu trực quan, món ăn bán chạy (Top Sellers), tỷ lệ lấp đầy bàn.
- **Chương trình Khuyến mãi (`/admin/vouchers`)**: Tạo và quản lý mã voucher giảm giá.

---

## 3. Tài Khoản Đăng Nhập Thử Nghiệm

| Vai trò | Email | Mật khẩu |
|---------|-------|----------|
| **Quản trị viên (Admin)** | `admin@restaurant.com` | `Admin@123456` |
| **Nhân viên (Staff)** | `staff@restaurant.com` | `Staff@123456` |
| **Khách hàng (Customer)** | `customer@restaurant.com` | `Customer@123456` |

---

## 4. Kiểm Thử & Đảm Bảo Chất Lượng

Hệ thống được kiểm thử tự động toàn diện thông qua **Chrome DevTools Protocol (CDP)** trên cả Desktop (1440x900) và Mobile (390x844):
- **Console Errors**: 0 lỗi.
- **Network Failures**: 0 lỗi (toàn bộ requests 200 OK).
- **Ảnh chụp màn hình thực tế**: Được lưu trữ đầy đủ trong thư mục `screenshots/`.
