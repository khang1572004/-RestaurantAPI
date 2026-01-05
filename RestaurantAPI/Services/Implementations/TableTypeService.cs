using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.TableType;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class TableTypeService : ITableTypeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TableTypeService> _logger;

    public TableTypeService(ApplicationDbContext context, ILogger<TableTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TableTypeDto>> GetAllTableTypesAsync(bool includeInactive = false)
    {
        var query = _context.TableTypes
            .Include(tt => tt.Tables)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(tt => tt.IsActive);
        }

        var types = await query
            .OrderBy(tt => tt.Name)
            .Select(tt => new TableTypeDto
            {
                Id = tt.Id,
                Name = tt.Name,
                DepositAmount = tt.DepositAmount,
                Description = tt.Description,
                IsActive = tt.IsActive,
                TablesCount = tt.Tables.Count,
                CreatedAt = tt.CreatedAt
            })
            .ToListAsync();

        return types;
    }

    public async Task<TableTypeDto?> GetTableTypeByIdAsync(int id)
    {
        var type = await _context.TableTypes
            .Include(tt => tt.Tables)
            .FirstOrDefaultAsync(tt => tt.Id == id);

        if (type == null)
            return null;

        return new TableTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            DepositAmount = type.DepositAmount,
            Description = type.Description,
            IsActive = type.IsActive,
            TablesCount = type.Tables.Count,
            CreatedAt = type.CreatedAt
        };
    }

    public async Task<(bool Success, string Message, TableTypeDto? Data)> CreateTableTypeAsync(CreateTableTypeDto dto)
    {
        try
        {
            if (await _context.TableTypes.AnyAsync(tt => tt.Name == dto.Name))
            {
                return (false, "Tên loại bàn đã tồn tại", null);
            }

            var tableType = new TableType
            {
                Name = dto.Name,
                DepositAmount = dto.DepositAmount,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TableTypes.Add(tableType);
            await _context.SaveChangesAsync();

            var result = await GetTableTypeByIdAsync(tableType.Id);

            _logger.LogInformation("TableType created: {TableTypeName}", tableType.Name);
            return (true, "Tạo loại bàn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table type");
            return (false, "Đã xảy ra lỗi khi tạo loại bàn", null);
        }
    }

    public async Task<(bool Success, string Message, TableTypeDto? Data)> UpdateTableTypeAsync(int id, UpdateTableTypeDto dto)
    {
        try
        {
            var tableType = await _context.TableTypes.FindAsync(id);
            if (tableType == null)
            {
                return (false, "Không tìm thấy loại bàn", null);
            }

            if (await _context.TableTypes.AnyAsync(tt => tt.Name == dto.Name && tt.Id != id))
            {
                return (false, "Tên loại bàn đã tồn tại", null);
            }

            tableType.Name = dto.Name;
            tableType.DepositAmount = dto.DepositAmount;
            tableType.Description = dto.Description;
            tableType.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var result = await GetTableTypeByIdAsync(id);

            _logger.LogInformation("TableType updated: {TableTypeId}", id);
            return (true, "Cập nhật loại bàn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table type {TableTypeId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật loại bàn", null);
        }
    }

    public async Task<(bool Success, string Message)> DeleteTableTypeAsync(int id)
    {
        try
        {
            var tableType = await _context.TableTypes
                .Include(tt => tt.Tables)
                .FirstOrDefaultAsync(tt => tt.Id == id);

            if (tableType == null)
            {
                return (false, "Không tìm thấy loại bàn");
            }

            if (tableType.Tables.Any())
            {
                return (false, "Không thể xóa loại bàn có bàn đang sử dụng. Vui lòng xóa các bàn trước.");
            }

            _context.TableTypes.Remove(tableType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("TableType deleted: {TableTypeId}", id);
            return (true, "Xóa loại bàn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting table type {TableTypeId}", id);
            return (false, "Đã xảy ra lỗi khi xóa loại bàn");
        }
    }
}