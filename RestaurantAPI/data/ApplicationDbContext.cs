using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ============================================
    // CONFIGURE WARNINGS
    // ============================================
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Tạm thời ignore warning về pending model changes
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
        );
    }

    // ============================================
    // DbSets
    // ============================================
    
    // User & Auth
    public DbSet<User> Users { get; set; }

    // Menu
    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuItemPriceHistory> MenuItemPriceHistories { get; set; }

    // Table
    public DbSet<Zone> Zones { get; set; }
    public DbSet<TableType> TableTypes { get; set; }
    public DbSet<Table> Tables { get; set; }

    // Booking
    public DbSet<Booking> Bookings { get; set; }

    // Payment
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentSetting> PaymentSettings { get; set; }
    public DbSet<RefundRequest> RefundRequests { get; set; }

    // Order
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Voucher
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherUsage> VoucherUsages { get; set; }

    // Review
    public DbSet<Review> Reviews { get; set; }

    // Staff
    public DbSet<StaffShift> StaffShifts { get; set; }

    // System
    public DbSet<RestaurantSetting> RestaurantSettings { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // INDEXES
        // ============================================

        // User Indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Phone);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailVerificationToken);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.PasswordResetToken);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.RefreshTokenHash)
            .IsUnique();

        // Booking Indexes
        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingCode)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingDate);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.CustomerId);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.Status);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.BookingDate, b.Status });

        // Table Indexes
        modelBuilder.Entity<Table>()
            .HasIndex(t => t.TableNumber);

        modelBuilder.Entity<Table>()
            .HasIndex(t => t.Status);

        modelBuilder.Entity<Table>()
            .HasIndex(t => new { t.ZoneId, t.Status });

        // Voucher Indexes
        modelBuilder.Entity<Voucher>()
            .HasIndex(v => v.Code)
            .IsUnique();

        modelBuilder.Entity<Voucher>()
            .HasIndex(v => v.IsActive);

        modelBuilder.Entity<Voucher>()
            .HasIndex(v => new { v.StartDate, v.EndDate });

        // Payment Indexes
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.Status);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.BookingId);

        // Order Indexes
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.BookingId);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt);

        // RestaurantSetting Indexes
        modelBuilder.Entity<RestaurantSetting>()
            .HasIndex(rs => rs.Key)
            .IsUnique();

        // EmailLog Indexes
        modelBuilder.Entity<EmailLog>()
            .HasIndex(e => e.EmailType);

        modelBuilder.Entity<EmailLog>()
            .HasIndex(e => e.IsSent);

        modelBuilder.Entity<EmailLog>()
            .HasIndex(e => e.CreatedAt);

        // AuditLog Indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.TableName);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.UserId);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.CreatedAt);

        // ============================================
        // RELATIONSHIPS
        // ============================================

        // User relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.Customer)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.Customer)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ProcessedRefunds)
            .WithOne(rr => rr.ProcessedByUser)
            .HasForeignKey(rr => rr.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.CreatedOrders)
            .WithOne(o => o.Staff)
            .HasForeignKey(o => o.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.StaffShifts)
            .WithOne(ss => ss.Staff)
            .HasForeignKey(ss => ss.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category relationships
        modelBuilder.Entity<Category>()
            .HasMany(c => c.MenuItems)
            .WithOne(mi => mi.Category)
            .HasForeignKey(mi => mi.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Zone relationships
        modelBuilder.Entity<Zone>()
            .HasMany(z => z.Tables)
            .WithOne(t => t.Zone)
            .HasForeignKey(t => t.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // TableType relationships
        modelBuilder.Entity<TableType>()
            .HasMany(tt => tt.Tables)
            .WithOne(t => t.TableType)
            .HasForeignKey(t => t.TableTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Table relationships
        modelBuilder.Entity<Table>()
            .HasMany(t => t.Bookings)
            .WithOne(b => b.Table)
            .HasForeignKey(b => b.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Voucher)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VoucherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Order)
            .WithOne(o => o.Booking)
            .HasForeignKey<Order>(o => o.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Review)
            .WithOne(r => r.Booking)
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasMany(b => b.Payments)
            .WithOne(p => p.Booking)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasMany(b => b.RefundRequests)
            .WithOne(rr => rr.Booking)
            .HasForeignKey(rr => rr.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment relationships
        modelBuilder.Entity<Payment>()
            .HasMany(p => p.RefundRequests)
            .WithOne(rr => rr.Payment)
            .HasForeignKey(rr => rr.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Table)
            .WithMany()
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Voucher)
            .WithMany(v => v.Orders)
            .HasForeignKey(o => o.VoucherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem relationships
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany(mi => mi.OrderItems)
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // MenuItem relationships
        modelBuilder.Entity<MenuItem>()
            .HasMany(mi => mi.PriceHistories)
            .WithOne(ph => ph.MenuItem)
            .HasForeignKey(ph => ph.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
            .HasOne(mi => mi.UpdatedByUser)
            .WithMany()
            .HasForeignKey(mi => mi.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // MenuItemPriceHistory relationships
        modelBuilder.Entity<MenuItemPriceHistory>()
            .HasOne(ph => ph.ChangedByUser)
            .WithMany()
            .HasForeignKey(ph => ph.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Voucher relationships
        modelBuilder.Entity<Voucher>()
            .HasMany(v => v.VoucherUsages)
            .WithOne(vu => vu.Voucher)
            .HasForeignKey(vu => vu.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // VoucherUsage relationships
        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Customer)
            .WithMany(u => u.VoucherUsages)
            .HasForeignKey(vu => vu.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Booking)
            .WithMany()
            .HasForeignKey(vu => vu.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Order)
            .WithMany()
            .HasForeignKey(vu => vu.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // EmailLog relationships
        modelBuilder.Entity<EmailLog>()
            .HasOne(e => e.User)
            .WithMany(u => u.EmailLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // AuditLog relationships
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ============================================
        // DECIMAL PRECISION
        // ============================================

        modelBuilder.Entity<MenuItem>()
            .Property(mi => mi.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TableType>()
            .Property(tt => tt.DepositAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Table>()
            .Property(t => t.CustomDepositAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DepositAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.FinalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Voucher>()
            .Property(v => v.DiscountValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Voucher>()
            .Property(v => v.MinOrderValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MenuItemPriceHistory>()
            .Property(ph => ph.OldPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MenuItemPriceHistory>()
            .Property(ph => ph.NewPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RefundRequest>()
            .Property(rr => rr.RefundAmount)
            .HasPrecision(18, 2);

        // ============================================
        // SEED DATA
        // ============================================

        // Seed default admin and staff users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "System Administrator",
                Email = "admin@restaurant.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                Phone = "0123456789",
                Role = UserRole.Admin,
                IsActive = true,
                IsEmailVerified = true,
                EmailVerificationToken = null,
                PasswordResetToken = null,
                PasswordResetExpiry = null,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastLoginAt = null
            },
            new User
            {
                Id = 2,
                FullName = "Restaurant Staff",
                Email = "staff@restaurant.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123456"),
                Phone = "0987654321",
                Role = UserRole.Staff,
                IsActive = true,
                IsEmailVerified = true,
                EmailVerificationToken = null,
                PasswordResetToken = null,
                PasswordResetExpiry = null,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastLoginAt = null
            },
            new User
            {
                Id = 3,
                FullName = "Demo Customer",
                Email = "customer@restaurant.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123456"),
                Phone = "0900000000",
                Role = UserRole.Customer,
                IsActive = true,
                IsEmailVerified = true,
                EmailVerificationToken = null,
                PasswordResetToken = null,
                PasswordResetExpiry = null,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastLoginAt = null
            }
        );

        // Seed default settings
        modelBuilder.Entity<RestaurantSetting>().HasData(
            new RestaurantSetting
            {
                Id = 1,
                Key = "restaurant_name",
                Value = "Nhà Hàng Cao Cấp",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RestaurantSetting
            {
                Id = 2,
                Key = "min_advance_hours",
                Value = "2",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RestaurantSetting
            {
                Id = 3,
                Key = "cancellation_hours",
                Value = "4",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RestaurantSetting
            {
                Id = 4,
                Key = "require_deposit",
                Value = "true",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RestaurantSetting
            {
                Id = 5,
                Key = "opening_hours",
                Value = "{\"mon\":\"9:00-22:00\",\"tue\":\"9:00-22:00\",\"wed\":\"9:00-22:00\",\"thu\":\"9:00-22:00\",\"fri\":\"9:00-23:00\",\"sat\":\"9:00-23:00\",\"sun\":\"9:00-22:00\"}",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Table Types
        modelBuilder.Entity<TableType>().HasData(
            new TableType
            {
                Id = 1,
                Name = "Standard",
                DepositAmount = 50000,
                Description = "Bàn tiêu chuẩn",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new TableType
            {
                Id = 2,
                Name = "VIP",
                DepositAmount = 100000,
                Description = "Bàn VIP không gian riêng tư",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new TableType
            {
                Id = 3,
                Name = "Rooftop",
                DepositAmount = 150000,
                Description = "Bàn sân thượng view đẹp",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Zones
        modelBuilder.Entity<Zone>().HasData(
            new Zone
            {
                Id = 1,
                Name = "Khu A - Tầng 1",
                Description = "Khu vực trong nhà tầng 1",
                Position = 1,
                IsActive = true
            },
            new Zone
            {
                Id = 2,
                Name = "Khu B - Tầng 2",
                Description = "Khu vực trong nhà tầng 2",
                Position = 2,
                IsActive = true
            },
            new Zone
            {
                Id = 3,
                Name = "Khu C - Rooftop",
                Description = "Khu vực sân thượng",
                Position = 3,
                IsActive = true
            }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Khai vị",
                Description = "Món khai vị",
                Position = 1,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = 2,
                Name = "Món chính",
                Description = "Món chính",
                Position = 2,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = 3,
                Name = "Đồ uống",
                Description = "Nước uống các loại",
                Position = 3,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = 4,
                Name = "Tráng miệng",
                Description = "Món tráng miệng",
                Position = 4,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Tables
        modelBuilder.Entity<Table>().HasData(
            new Table { Id = 1, TableNumber = "Bàn 01", ZoneId = 1, TableTypeId = 1, Capacity = 4, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 2, TableNumber = "Bàn 02", ZoneId = 1, TableTypeId = 1, Capacity = 4, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 3, TableNumber = "Bàn 03", ZoneId = 1, TableTypeId = 1, Capacity = 2, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 4, TableNumber = "VIP 01", ZoneId = 2, TableTypeId = 2, Capacity = 8, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 5, TableNumber = "VIP 02", ZoneId = 2, TableTypeId = 2, Capacity = 6, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 6, TableNumber = "Rooftop 01", ZoneId = 3, TableTypeId = 3, Capacity = 4, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Table { Id = 7, TableNumber = "Rooftop 02", ZoneId = 3, TableTypeId = 3, Capacity = 4, Status = TableStatus.Available, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Menu Items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, CategoryId = 1, Name = "Salad Hoàng Đế (Caesar)", Description = "Xà lách Romaine tươi giòn, sốt Caesar, thịt xông khói & phô mai Parmesan", Price = 89000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 2, CategoryId = 1, Name = "Súp Bào Ngư Nấm Đông Cô", Description = "Súp dinh dưỡng thượng hạng nấu từ nấm đông cô tươi và nước hầm gà", Price = 129000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 3, CategoryId = 2, Name = "Bò Wagyu Bít Tết Sốt Tiêu Đen", Description = "Thăn bò Wagyu nướng mềm mọng nước dùng kèm sốt tiêu đen hảo hạng", Price = 380000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 4, CategoryId = 2, Name = "Cá Hồi Nướng Sốt Chanh Dây", Description = "Phi lê cá hồi Na Uy áp chảo sốt chanh leo chua ngọt, măng tây nướng", Price = 265000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 5, CategoryId = 2, Name = "Mì Ý Tôm Hùm Sốt Kem Truffle", Description = "Mì Ý tươi sợi dẹt sốt kem nấm Truffle đen thơm béo ngậy kèm đuôi tôm hùm", Price = 320000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 6, CategoryId = 3, Name = "Trà Hoa Cúc Mật Ong", Description = "Trà thảo mộc hoa cúc thanh mát dịu ngọt", Price = 45000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 7, CategoryId = 3, Name = "Nước Ép Dưa Hấu Bạc Hà", Description = "Nước ép nguyên chất dưa hấu tươi mát lạnh", Price = 50000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MenuItem { Id = 8, CategoryId = 4, Name = "Bánh Tiramisu Truyền Thống", Description = "Bánh Tiramisu vị cà phê phô mai Mascarpone béo mịn", Price = 65000, IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
