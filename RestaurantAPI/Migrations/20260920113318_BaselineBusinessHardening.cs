using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantAPI.Migrations
{
    /// <inheritdoc />
    public partial class BaselineBusinessHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    SecretKey = table.Column<string>(type: "text", nullable: false),
                    ReturnUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepositAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasswordResetToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasswordResetExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DiscountType = table.Column<int>(type: "integer", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinOrderValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxUsagePerCustomer = table.Column<int>(type: "integer", nullable: false),
                    TotalUsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    EmailTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    EmailType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItems_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StaffShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffShifts_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ZoneId = table.Column<int>(type: "integer", nullable: true),
                    TableTypeId = table.Column<int>(type: "integer", nullable: false),
                    TableNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CustomDepositAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MaintenanceNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tables_TableTypes_TableTypeId",
                        column: x => x.TableTypeId,
                        principalTable: "TableTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tables_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChangedBy = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemPriceHistories_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemPriceHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItemPriceHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BookingTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    NumGuests = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BookingCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    VoucherId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ServiceDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsLateCheckIn = table.Column<bool>(type: "boolean", nullable: false),
                    DepositForfeited = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: true),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VoucherId = table.Column<int>(type: "integer", nullable: true),
                    DepositAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentResponse = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoucherUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BookingId = table.Column<int>(type: "integer", nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherUsages_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VoucherUsages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VoucherUsages_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherUsages_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProcessedBy = table.Column<int>(type: "integer", nullable: true),
                    AdminNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Position" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Món khai vị", true, "Khai vị", 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Món chính", true, "Món chính", 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nước uống các loại", true, "Đồ uống", 3 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Món tráng miệng", true, "Tráng miệng", 4 }
                });

            migrationBuilder.InsertData(
                table: "RestaurantSettings",
                columns: new[] { "Id", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "restaurant_name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhà Hàng Cao Cấp" },
                    { 2, "min_advance_hours", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2" },
                    { 3, "cancellation_hours", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "4" },
                    { 4, "require_deposit", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "true" },
                    { 5, "opening_hours", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"mon\":\"9:00-22:00\",\"tue\":\"9:00-22:00\",\"wed\":\"9:00-22:00\",\"thu\":\"9:00-22:00\",\"fri\":\"9:00-23:00\",\"sat\":\"9:00-23:00\",\"sun\":\"9:00-22:00\"}" }
                });

            migrationBuilder.InsertData(
                table: "TableTypes",
                columns: new[] { "Id", "CreatedAt", "DepositAmount", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, "Bàn tiêu chuẩn", true, "Standard" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100000m, "Bàn VIP không gian riêng tư", true, "VIP" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 150000m, "Bàn sân thượng view đẹp", true, "Rooftop" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "EmailVerificationToken", "FullName", "IsActive", "IsEmailVerified", "LastLoginAt", "PasswordHash", "PasswordResetExpiry", "PasswordResetToken", "Phone", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@restaurant.com", null, "System Administrator", true, true, null, "$2a$11$fYo4DFIphDGYRLhqc/uYXersvG0FefZlERZjC7fpWHoRTQSXQm//S", null, null, "0123456789", 3 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff@restaurant.com", null, "Restaurant Staff", true, true, null, "$2a$11$TPDsmG29yinTxO6/cKxhWejF2gjTsrEsQCLE2bGrr2bLja8RxNkgK", null, null, "0987654321", 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer@restaurant.com", null, "Demo Customer", true, true, null, "$2a$11$Z3wI5Pl5El/rGvvAAtLOtOQ4Hg3t2lCynhBmxNQ6cx1Ka6cWiU.R6", null, null, "0900000000", 1 }
                });

            migrationBuilder.InsertData(
                table: "Zones",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Position" },
                values: new object[,]
                {
                    { 1, "Khu vực trong nhà tầng 1", true, "Khu A - Tầng 1", 1 },
                    { 2, "Khu vực trong nhà tầng 2", true, "Khu B - Tầng 2", 2 },
                    { 3, "Khu vực sân thượng", true, "Khu C - Rooftop", 3 }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "Position", "Price", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xà lách Romaine tươi giòn, sốt Caesar, thịt xông khói & phô mai Parmesan", null, true, "Salad Hoàng Đế (Caesar)", 0, 89000m, null, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Súp dinh dưỡng thượng hạng nấu từ nấm đông cô tươi và nước hầm gà", null, true, "Súp Bào Ngư Nấm Đông Cô", 0, 129000m, null, null },
                    { 3, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thăn bò Wagyu nướng mềm mọng nước dùng kèm sốt tiêu đen hảo hạng", null, true, "Bò Wagyu Bít Tết Sốt Tiêu Đen", 0, 380000m, null, null },
                    { 4, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phi lê cá hồi Na Uy áp chảo sốt chanh leo chua ngọt, măng tây nướng", null, true, "Cá Hồi Nướng Sốt Chanh Dây", 0, 265000m, null, null },
                    { 5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mì Ý tươi sợi dẹt sốt kem nấm Truffle đen thơm béo ngậy kèm đuôi tôm hùm", null, true, "Mì Ý Tôm Hùm Sốt Kem Truffle", 0, 320000m, null, null },
                    { 6, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trà thảo mộc hoa cúc thanh mát dịu ngọt", null, true, "Trà Hoa Cúc Mật Ong", 0, 45000m, null, null },
                    { 7, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nước ép nguyên chất dưa hấu tươi mát lạnh", null, true, "Nước Ép Dưa Hấu Bạc Hà", 0, 50000m, null, null },
                    { 8, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bánh Tiramisu vị cà phê phô mai Mascarpone béo mịn", null, true, "Bánh Tiramisu Truyền Thống", 0, 65000m, null, null }
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Capacity", "CreatedAt", "CustomDepositAmount", "IsActive", "LastMaintenanceDate", "MaintenanceNote", "Status", "TableNumber", "TableTypeId", "ZoneId" },
                values: new object[,]
                {
                    { 1, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "Bàn 01", 1, 1 },
                    { 2, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "Bàn 02", 1, 1 },
                    { 3, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "Bàn 03", 1, 1 },
                    { 4, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "VIP 01", 2, 2 },
                    { 5, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "VIP 02", 2, 2 },
                    { 6, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "Rooftop 01", 3, 3 },
                    { 7, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, 1, "Rooftop 02", 3, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName",
                table: "AuditLogs",
                column: "TableName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingCode",
                table: "Bookings",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingDate",
                table: "Bookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingDate_Status",
                table: "Bookings",
                columns: new[] { "BookingDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TableId",
                table: "Bookings",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VoucherId",
                table: "Bookings",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_CreatedAt",
                table: "EmailLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailType",
                table: "EmailLogs",
                column: "EmailType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_IsSent",
                table: "EmailLogs",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_UserId",
                table: "EmailLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemPriceHistories_ChangedBy",
                table: "MenuItemPriceHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemPriceHistories_MenuItemId",
                table: "MenuItemPriceHistories",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemPriceHistories_UserId",
                table: "MenuItemPriceHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_UpdatedBy",
                table: "MenuItems",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BookingId",
                table: "Orders",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StaffId",
                table: "Orders",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VoucherId",
                table: "Orders",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_BookingId",
                table: "RefundRequests",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_PaymentId",
                table: "RefundRequests",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_ProcessedBy",
                table: "RefundRequests",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantSettings_Key",
                table: "RestaurantSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId",
                table: "StaffShifts",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_Status",
                table: "Tables",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableNumber",
                table: "Tables",
                column: "TableNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableTypeId",
                table: "Tables",
                column: "TableTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_ZoneId_Status",
                table: "Tables",
                columns: new[] { "ZoneId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users",
                column: "EmailVerificationToken");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users",
                column: "PasswordResetToken");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_IsActive",
                table: "Vouchers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_StartDate_EndDate",
                table: "Vouchers",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_BookingId",
                table: "VoucherUsages",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_CustomerId",
                table: "VoucherUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_OrderId",
                table: "VoucherUsages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_VoucherId",
                table: "VoucherUsages",
                column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "MenuItemPriceHistories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "PaymentSettings");

            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropTable(
                name: "RestaurantSettings");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "StaffShifts");

            migrationBuilder.DropTable(
                name: "VoucherUsages");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "TableTypes");

            migrationBuilder.DropTable(
                name: "Zones");
        }
    }
}
