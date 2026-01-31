using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantAPI.Middleware;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Implementations;
using RestaurantAPI.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ DEBUG: Check config được load
var minHours = builder.Configuration.GetValue<int>("BookingSettings:MinAdvanceHours", 999);
Console.WriteLine($"🔍 DEBUG: MinAdvanceHours from config = {minHours}");

// ===== Database Context =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    );
    
    if (builder.Environment.IsDevelopment())
    {
        options.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
        );
    }
});

// ===== Controllers =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

// ===== JWT Authentication =====
var jwtKey = builder.Configuration["Jwt:Key"] ?? "KhangSuperSecretKeyThatIsVeryLong1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RestaurantAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RestaurantClient";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

// ===== Swagger/OpenAPI =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Restaurant Booking API",
        Version = "v1",
        Description = "API quản lý đặt bàn nhà hàng"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer {token}' để authenticate"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// ===== 🆕 REGISTER SERVICES =====
// Phase 1 - Authentication
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Phase 2 - Restaurant & Menu Management
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<ITableTypeService, TableTypeService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IFileService, FileService>();

// Phase 3 - Booking System
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITableAvailabilityService, TableAvailabilityService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

// Phase 4 - Payment Integration
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IMoMoService, MoMoService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Phase 5 - Order & Review Management 
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Phase 6 - Staff, Voucher & Reporting 
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add HttpClient for MoMo
builder.Services.AddHttpClient();

// ===== HTTP Client for Payment Gateways =====
builder.Services.AddHttpClient("VNPay");
builder.Services.AddHttpClient("MoMo");

// ===== Memory Cache =====
builder.Services.AddMemoryCache();

// ===== Build App =====
var app = builder.Build();

// ===== 🆕 USE EXCEPTION MIDDLEWARE =====
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();