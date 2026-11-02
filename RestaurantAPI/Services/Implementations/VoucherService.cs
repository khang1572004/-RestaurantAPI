using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Voucher;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class VoucherService : IVoucherService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VoucherService> _logger;

    public VoucherService(ApplicationDbContext context, ILogger<VoucherService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // EXISTING METHODS (Phase 3)
    // ============================================

    public async Task<List<VoucherDto>> GetActiveVouchersAsync()
    {
        var now = DateTime.UtcNow;

        var vouchers = await _context.Vouchers
            .Where(v => v.IsActive && v.StartDate <= now && v.EndDate >= now)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                DiscountType = v.DiscountType.ToString(),
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                MaxUsagePerCustomer = v.MaxUsagePerCustomer,
                TotalUsageLimit = v.TotalUsageLimit,
                UsedCount = v.UsedCount,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                IsActive = v.IsActive,
                IsAvailable = v.IsActive && 
                             v.StartDate <= now && 
                             v.EndDate >= now &&
                             (!v.TotalUsageLimit.HasValue || v.UsedCount < v.TotalUsageLimit.Value)
            })
            .ToListAsync();

        return vouchers;
    }

    public async Task<VoucherValidationResult> ValidateVoucherAsync(
        string code,
        int customerId,
        decimal? orderValue = null)
    {
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code.ToUpper() == code.Trim().ToUpper());

        if (voucher == null)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = "Mã voucher không tồn tại"
            };
        }

        if (!voucher.IsActive)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = "Voucher đã bị vô hiệu hóa"
            };
        }

        var now = DateTime.UtcNow;
        if (now < voucher.StartDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = $"Voucher chưa có hiệu lực. Bắt đầu từ {voucher.StartDate:dd/MM/yyyy}"
            };
        }

        if (now > voucher.EndDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = "Voucher đã hết hạn"
            };
        }

        if (voucher.TotalUsageLimit.HasValue && voucher.UsedCount >= voucher.TotalUsageLimit.Value)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = "Voucher đã hết lượt sử dụng"
            };
        }

        var canUse = await CanCustomerUseVoucherAsync(voucher.Id, customerId);
        if (!canUse)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = $"Bạn đã sử dụng voucher này đủ số lần cho phép ({voucher.MaxUsagePerCustomer} lần)"
            };
        }

        if (orderValue.HasValue && orderValue.Value < voucher.MinOrderValue)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                Message = $"Giá trị đơn hàng tối thiểu là {voucher.MinOrderValue:N0}đ"
            };
        }

        decimal? discountAmount = null;
        if (orderValue.HasValue)
        {
            discountAmount = await CalculateDiscountAsync(voucher, orderValue.Value);
        }

        return new VoucherValidationResult
        {
            IsValid = true,
            Message = "Voucher hợp lệ",
            Voucher = new VoucherDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                DiscountType = voucher.DiscountType.ToString(),
                DiscountValue = voucher.DiscountValue,
                MinOrderValue = voucher.MinOrderValue,
                MaxUsagePerCustomer = voucher.MaxUsagePerCustomer,
                TotalUsageLimit = voucher.TotalUsageLimit,
                UsedCount = voucher.UsedCount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                IsActive = voucher.IsActive,
                IsAvailable = true
            },
            DiscountAmount = discountAmount
        };
    }

    public async Task<decimal> CalculateDiscountAsync(Voucher voucher, decimal orderValue)
    {
        if (voucher.DiscountType == DiscountType.Percentage)
        {
            return Math.Min(orderValue, orderValue * (voucher.DiscountValue / 100));
        }
        else
        {
            return Math.Min(voucher.DiscountValue, orderValue);
        }
    }

    public async Task<bool> CanCustomerUseVoucherAsync(int voucherId, int customerId)
    {
        var usageCount = await _context.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucherId && vu.CustomerId == customerId);

        var voucher = await _context.Vouchers.FindAsync(voucherId);
        if (voucher == null)
            return false;

        return usageCount < voucher.MaxUsagePerCustomer;
    }

    public async Task RecordVoucherUsageAsync(
        int voucherId,
        int customerId,
        int? bookingId = null,
        int? orderId = null)
    {
        var usage = new VoucherUsage
        {
            VoucherId = voucherId,
            CustomerId = customerId,
            BookingId = bookingId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow
        };

        _context.VoucherUsages.Add(usage);

        var voucher = await _context.Vouchers.FindAsync(voucherId);
        if (voucher != null)
        {
            voucher.UsedCount++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Voucher {VoucherId} used by customer {CustomerId}", voucherId, customerId);
    }

    // ============================================
    // ⭐ NEW METHODS - CRUD OPERATIONS (Phase 6)
    // ============================================

    // CN080: Get all vouchers với filter
    public async Task<(List<VoucherDto> Vouchers, int TotalCount)> GetAllVouchersAsync(VoucherFilterDto filter)
    {
        var now = DateTime.UtcNow;
        var query = _context.Vouchers.AsQueryable();

        // Apply filters
        if (filter.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == filter.IsActive.Value);
        }

        if (filter.IsExpired.HasValue)
        {
            if (filter.IsExpired.Value)
            {
                query = query.Where(v => v.EndDate < now);
            }
            else
            {
                query = query.Where(v => v.EndDate >= now);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(v =>
                v.Code.ToLower().Contains(searchTerm) ||
                v.Name.ToLower().Contains(searchTerm)
            );
        }

        var totalCount = await query.CountAsync();

        var vouchers = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                DiscountType = v.DiscountType.ToString(),
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                MaxUsagePerCustomer = v.MaxUsagePerCustomer,
                TotalUsageLimit = v.TotalUsageLimit,
                UsedCount = v.UsedCount,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                IsActive = v.IsActive,
                IsAvailable = v.IsActive && 
                             v.StartDate <= now && 
                             v.EndDate >= now &&
                             (!v.TotalUsageLimit.HasValue || v.UsedCount < v.TotalUsageLimit.Value)
            })
            .ToListAsync();

        return (vouchers, totalCount);
    }

    public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
    {
        var now = DateTime.UtcNow;
        var voucher = await _context.Vouchers
            .Where(v => v.Id == id)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                DiscountType = v.DiscountType.ToString(),
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                MaxUsagePerCustomer = v.MaxUsagePerCustomer,
                TotalUsageLimit = v.TotalUsageLimit,
                UsedCount = v.UsedCount,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                IsActive = v.IsActive,
                IsAvailable = v.IsActive && 
                             v.StartDate <= now && 
                             v.EndDate >= now &&
                             (!v.TotalUsageLimit.HasValue || v.UsedCount < v.TotalUsageLimit.Value)
            })
            .FirstOrDefaultAsync();

        return voucher;
    }

    // CN076: Create voucher
    public async Task<(bool Success, string Message, VoucherDto? Data)> CreateVoucherAsync(CreateVoucherDto dto)
    {
        // Validate code uniqueness
        var codeExists = await _context.Vouchers.AnyAsync(v => v.Code.ToUpper() == dto.Code.ToUpper());
        if (codeExists)
        {
            return (false, "Mã voucher đã tồn tại", null);
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            return (false, "Ngày kết thúc phải sau ngày bắt đầu", null);
        }

        var voucher = new Voucher
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinOrderValue = dto.MinOrderValue,
            MaxUsagePerCustomer = dto.MaxUsagePerCustomer,
            TotalUsageLimit = dto.TotalUsageLimit,
            UsedCount = 0,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created voucher: {Code}", dto.Code);

        var voucherDto = await GetVoucherByIdAsync(voucher.Id);
        return (true, "Tạo voucher thành công", voucherDto);
    }

    // CN077: Update voucher
    public async Task<(bool Success, string Message, VoucherDto? Data)> UpdateVoucherAsync(int id, UpdateVoucherDto dto)
    {
        var voucher = await _context.Vouchers.FindAsync(id);

        if (voucher == null)
        {
            return (false, "Voucher không tồn tại", null);
        }

        // Không cho sửa nếu đã có người dùng
        if (voucher.UsedCount > 0)
        {
            return (false, "Không thể sửa voucher đã được sử dụng", null);
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            return (false, "Ngày kết thúc phải sau ngày bắt đầu", null);
        }

        voucher.Name = dto.Name;
        voucher.DiscountType = dto.DiscountType;
        voucher.DiscountValue = dto.DiscountValue;
        voucher.MinOrderValue = dto.MinOrderValue;
        voucher.MaxUsagePerCustomer = dto.MaxUsagePerCustomer;
        voucher.TotalUsageLimit = dto.TotalUsageLimit;
        voucher.StartDate = dto.StartDate;
        voucher.EndDate = dto.EndDate;
        voucher.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated voucher: {VoucherId}", id);

        var voucherDto = await GetVoucherByIdAsync(id);
        return (true, "Cập nhật voucher thành công", voucherDto);
    }

    // CN078: Delete voucher
    public async Task<(bool Success, string Message)> DeleteVoucherAsync(int id)
    {
        var voucher = await _context.Vouchers.FindAsync(id);

        if (voucher == null)
        {
            return (false, "Voucher không tồn tại");
        }

        // Soft delete
        voucher.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted voucher: {VoucherId}", id);

        return (true, "Xóa voucher thành công");
    }

    // CN079: Toggle voucher
    public async Task<(bool Success, string Message)> ToggleVoucherAsync(int id, bool isActive)
    {
        var voucher = await _context.Vouchers.FindAsync(id);

        if (voucher == null)
        {
            return (false, "Voucher không tồn tại");
        }

        voucher.IsActive = isActive;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled voucher {VoucherId} to {Status}", id, isActive ? "active" : "inactive");

        return (true, $"Voucher đã được {(isActive ? "kích hoạt" : "vô hiệu hóa")}");
    }

    // CN081: Get voucher stats
    public async Task<VoucherStatsDto> GetVoucherStatsAsync(int voucherId)
    {
        var voucher = await _context.Vouchers.FindAsync(voucherId);

        if (voucher == null)
        {
            throw new ArgumentException("Voucher không tồn tại");
        }

        var usages = await _context.VoucherUsages
            .Include(vu => vu.Customer)
            .Include(vu => vu.Order)
            .Where(vu => vu.VoucherId == voucherId)
            .ToListAsync();

        var totalDiscountAmount = usages
            .Where(vu => vu.Order != null)
            .Sum(vu => vu.Order!.DiscountAmount);

        var uniqueCustomers = usages.Select(vu => vu.CustomerId).Distinct().Count();

        var lastUsedAt = usages.OrderByDescending(vu => vu.UsedAt).FirstOrDefault()?.UsedAt;

        var topUsers = usages
            .GroupBy(vu => new { vu.CustomerId, vu.Customer.FullName, vu.Customer.Email })
            .Select(g => new TopVoucherUserDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.FullName,
                CustomerEmail = g.Key.Email,
                UsageCount = g.Count(),
                TotalSaved = g.Where(vu => vu.Order != null).Sum(vu => vu.Order!.DiscountAmount)
            })
            .OrderByDescending(u => u.UsageCount)
            .Take(10)
            .ToList();

        return new VoucherStatsDto
        {
            VoucherId = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            UsedCount = voucher.UsedCount,
            TotalUsageLimit = voucher.TotalUsageLimit,
            TotalDiscountAmount = totalDiscountAmount,
            UniqueCustomers = uniqueCustomers,
            LastUsedAt = lastUsedAt,
            TopUsers = topUsers
        };
    }
}
