# Restaurant Master - Customer Portal & Admin/Staff System

Hệ thống giao diện toàn diện cho Nhà hàng bao gồm **Customer Portal** (dành cho thực khách đặt bàn, xem thực đơn, xem lịch sử đặt bàn) và **Admin & Staff Portal** (sơ đồ bàn thời gian thực, điểm bán POS, quản lý nhân viên, hoàn tiền, kiểm duyệt đánh giá, khuyến mãi, báo cáo doanh thu).

Xây dựng trên nền tảng **React 19 + TypeScript + Vite + Tailwind CSS v4** kết hợp **TanStack Query v5 + Zustand** và tuân thủ quy chuẩn thiết kế **UI/UX Pro Max**.

---

## 1. Cấu Trúc Phân Hệ Giao Diện

### A. Customer Portal (Cổng Khách Hàng)
- **Trang chủ (`/`)**: Giới thiệu không gian nhà hàng chuẩn Fine Dining, danh mục món ăn đặc sắc được bếp trưởng tuyển chọn, thống kê đánh giá, CTA đặt bàn tức thì.
- **Thực đơn trực tuyến (`/menu`)**: Khám phá danh mục món ăn phong phú, giá bán niêm yết rõ ràng theo VND, lọc theo danh mục.
- **Đặt bàn trực tuyến 3 bước (`/book-table`)**:
  1. Chọn ngày, giờ và số lượng khách.
  2. Hệ thống kiểm tra thời gian thực và hiển thị các bàn trống còn khả dụng theo từng khu vực (Indoor, Garden, VIP, Terrace).
  3. Ghi chú yêu cầu đặc biệt và xác nhận đặt bàn tức thì.
- **Lịch sử đặt bàn cá nhân (`/my-bookings`)**: Quản lý lịch sử đặt chỗ, trạng thái (Chờ xác nhận, Đã xác nhận, Đã hủy, Hoàn thành), hủy bàn kèm lý do hoặc gửi đánh giá trải nghiệm sau bữa ăn.

### B. Staff Portal (Cổng Nhân Viên)
- **Sơ đồ bàn thời gian thực (`/tables`)**:
  - Trực quan hóa trạng thái bàn theo 4 màu chuẩn (Trống - Emerald, Đã đặt - Amber, Đang dùng bữa - Rose, Cần dọn - Slate).
  - Lọc nhanh theo từng khu vực (Indoor, Garden, VIP, Terrace...).
  - Thao tác nhanh: Check-in khách đến (hỗ trợ cả check-in trễ giờ), Mở bàn gọi món cho khách vãng lai, chuyển hướng POS.
- **Màn hình Điểm bán POS (`/pos`)**:
  - Danh mục món ăn, tìm kiếm nhanh, hình ảnh món và giá bán VND.
  - Hóa đơn bàn trực tiếp: Tăng/giảm số lượng món, ghi chú bếp, áp dụng mã voucher, tính trừ tiền cọc bàn.
  - Thanh toán đa phương thức (Tiền mặt, Quẹt thẻ POS, VNPay QR, MoMo QR).
- **Quản lý Đặt bàn & Check-in (`/bookings`)**:
  - Xem danh sách đặt bàn hôm nay và lọc theo ngày.
  - Phân quyền Admin duyệt/từ chối/hủy đặt bàn có ghi nhận lý do.
  - Check-in tự động cập nhật trạng thái bàn sang sơ đồ.
- **Ca trực nhân viên (`/shifts`)**:
  - Bắt đầu ca trực, theo dõi doanh thu trong ca, ghi chú bàn giao ca và kết ca an toàn.

### C. Admin Portal (Cổng Quản Trị Nâng Cao)
- **Quản lý Nhân sự (`/admin/staff`)**:
  - Danh sách nhân viên, vai trò (Admin, Staff, Manager), số điện thoại, trạng thái tài khoản.
  - Cấp tài khoản nhân viên mới, phân quyền truy cập.
- **Quản lý Yêu cầu Hoàn tiền (`/admin/refunds`)**:
  - Danh sách yêu cầu hoàn cọc đặt bàn, trạng thái (Chờ xử lý, Đã duyệt, Đã từ chối).
  - Duyệt hoàn tiền chuyển khoản hoặc từ chối kèm lý do minh bạch.
- **Kiểm duyệt Đánh giá (`/admin/reviews`)**:
  - Danh sách đánh giá của khách hàng từ 1 đến 5 sao.
  - Tính năng ẩn/hiện hoặc duyệt hiển thị đánh giá công khai trên trang chủ.
- **Quản lý Thực đơn & Danh mục (`/admin/menu`)**:
  - Thêm, sửa, xóa món ăn, upload hình ảnh, đặt món đặc biệt của bếp trưởng.
  - Bật/tắt trạng thái Còn hàng / Hết hàng ngay lập tức.
  - Quản lý danh mục món và thứ tự hiển thị.
- **Quản lý Bàn & Khu vực (`/admin/tables-management`)**:
  - Thêm bàn, gán khu vực (Zone) và loại bàn (TableType).
- **Báo cáo & Phân tích Doanh thu (`/admin/reports`)**:
  - Biểu đồ biến động doanh thu theo ngày.
  - Bảng xếp hạng Top 10 món ăn bán chạy nhất (Best Sellers).
  - Thống kê tỷ lệ lấp đầy bàn và giá trị trung bình/đơn.
- **Quản lý Khuyến mãi (`/admin/vouchers`)**:
  - Tạo voucher theo % hoặc số tiền cố định, thời hạn sử dụng, kích hoạt/tạm dừng.

---

## 2. Cấu Hình Cơ Sở Dữ Liệu PostgreSQL

Backend sử dụng PostgreSQL (Npgsql). Connection string mặc định trong `RestaurantAPI/appsettings.Development.json` là:

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=restaurant_booking_dev;Username=postgres;Password=postgres"
```

### Chạy PostgreSQL bằng Docker
Từ thư mục gốc repo:

```bash
docker compose up -d postgres
```

Cấu hình mặc định:
- Host: `localhost`
- Port: `5432`
- Database: `restaurant_booking_dev`
- Username: `postgres`
- Password: `postgres`

Backend tự động seed dữ liệu mẫu đầy đủ (bàn, danh mục, món ăn, tài khoản mẫu) khi khởi động trong môi trường Development.

---

## 3. Hướng Dẫn Khởi Chạy

### Khởi chạy Backend (`RestaurantAPI`)
```bash
cd RestaurantAPI
dotnet run
# API chạy tại: http://localhost:5058
# Swagger API docs tại: http://localhost:5058/swagger
```

### Khởi chạy Frontend (`restaurant-admin-ui`)
```bash
cd restaurant-admin-ui
npm install
npm run dev
# Giao diện chạy tại: http://localhost:5173
```

---

## 4. Tài Khoản Đăng Nhập Mẫu

Trang Đăng nhập (`/login`) hỗ trợ nút chọn nhanh tài khoản thử nghiệm:

| Vai trò | Email | Mật khẩu | Quyền hạn chính |
|---------|-------|----------|-----------------|
| **Quản trị viên (Admin)** | `admin@restaurant.com` | `Admin@123456` | Toàn quyền Dashboard, Báo cáo, Quản lý Nhân sự, Hoàn tiền, Đánh giá, Thực đơn, Bàn, Khuyến mãi |
| **Nhân viên (Staff)** | `staff@restaurant.com` | `Staff@123456` | Quản lý sơ đồ bàn, Điểm bán POS, Check-in đặt bàn, Ca trực |
| **Khách hàng (Customer)** | `customer@restaurant.com` | `Customer@123456` | Đặt bàn trực tuyến, Xem thực đơn, Quản lý bàn đã đặt của tôi |

---

## 5. Kết Quả Kiểm Thử Tự Động (Automated Testing)

Hệ thống đã trải qua quy trình kiểm thử tự động toàn diện qua **Chrome DevTools Protocol (CDP)** trên cả 2 độ phân giải:
- **Desktop**: 1440 x 900 px
- **Mobile**: 390 x 844 px (iPhone 14 standard)

### Thống kê kiểm thử:
- **Total Console Errors/Warnings**: `0` (Không phát sinh bất kỳ lỗi console nào)
- **Total Network Failures (HTTP >= 400)**: `0` (Mọi API call đều trả về HTTP 200 OK)
- **Ảnh chụp màn hình thực tế**: Đã lưu trữ tại thư mục `screenshots/`:
  - `01_login_desktop.png` & `01_login_mobile.png`: Giao diện đăng nhập
  - `02_customer_home_desktop.png` & `02_customer_home_mobile.png`: Trang chủ khách hàng
  - `03_customer_book_desktop.png` & `03_customer_book_mobile.png`: Quy trình đặt bàn khách hàng
  - `04_customer_my_bookings_desktop.png`: Lịch sử đặt bàn
  - `05_admin_tables_dashboard.png` & `05_admin_tables_mobile.png`: Sơ đồ bàn thời gian thực
  - `06_admin_pos_screen.png`: Màn hình điểm bán POS
  - `07_admin_staff_management.png`: Quản trị nhân sự
  - `08_admin_refunds_management.png`: Quản lý yêu cầu hoàn tiền
  - `09_admin_reviews_moderation.png`: Kiểm duyệt đánh giá khách hàng
  - `10_admin_vouchers.png`: Quản lý voucher & khuyến mãi
  - `11_admin_reports.png`: Báo cáo phân tích doanh thu
