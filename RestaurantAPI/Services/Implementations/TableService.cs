using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Restaurant;
using RestaurantAPI.DTOs.Table;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class TableService : ITableService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TableService> _logger;

    public TableService(ApplicationDbContext context, ILogger<TableService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TableDto>> GetAllTablesAsync(int? zoneId = null, int? tableTypeId = null, TableStatus? status = null)
    {
        var query = _context.Tables
            .Include(t => t.Zone)
            .Include(t => t.TableType)
            .AsQueryable();

        if (zoneId.HasValue)
        {
            query = query.Where(t => t.ZoneId == zoneId.Value);
        }

        if (tableTypeId.HasValue)
        {
            query = query.Where(t => t.TableTypeId == tableTypeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var tables = await query
            .OrderBy(t => t.Zone != null ? t.Zone.Position : 999)
            .ThenBy(t => t.TableNumber)
            .Select(t => new TableDto
            {
                Id = t.Id,
                ZoneId = t.ZoneId,
                ZoneName = t.Zone != null ? t.Zone.Name : null,
                TableTypeId = t.TableTypeId,
                TableTypeName = t.TableType.Name,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                CustomDepositAmount = t.CustomDepositAmount,
                EffectiveDepositAmount = t.CustomDepositAmount ?? t.TableType.DepositAmount,
                Status = t.Status.ToString(),
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                LastMaintenanceDate = t.LastMaintenanceDate,
                MaintenanceNote = t.MaintenanceNote
            })
            .ToListAsync();

        return tables;
    }

    public async Task<TableDto?> GetTableByIdAsync(int id)
    {
        var table = await _context.Tables
            .Include(t => t.Zone)
            .Include(t => t.TableType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (table == null)
            return null;

        return new TableDto
        {
            Id = table.Id,
            ZoneId = table.ZoneId,
            ZoneName = table.Zone?.Name,
            TableTypeId = table.TableTypeId,
            TableTypeName = table.TableType.Name,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            CustomDepositAmount = table.CustomDepositAmount,
            EffectiveDepositAmount = table.CustomDepositAmount ?? table.TableType.DepositAmount,
            Status = table.Status.ToString(),
            IsActive = table.IsActive,
            CreatedAt = table.CreatedAt,
            LastMaintenanceDate = table.LastMaintenanceDate,
            MaintenanceNote = table.MaintenanceNote
        };
    }

    public async Task<(bool Success, string Message, TableDto? Data)> CreateTableAsync(CreateTableDto dto)
    {
        try
        {
            // Check table type exists
            if (!await _context.TableTypes.AnyAsync(tt => tt.Id == dto.TableTypeId))
            {
                return (false, "Loại bàn không tồn tại", null);
            }

            // Check zone exists (if provided)
            if (dto.ZoneId.HasValue && !await _context.Zones.AnyAsync(z => z.Id == dto.ZoneId.Value))
            {
                return (false, "Khu vực không tồn tại", null);
            }

            // Check duplicate table number
            if (await _context.Tables.AnyAsync(t => t.TableNumber == dto.TableNumber))
            {
                return (false, "Số bàn đã tồn tại", null);
            }

            var table = new Table
            {
                ZoneId = dto.ZoneId,
                TableTypeId = dto.TableTypeId,
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity,
                CustomDepositAmount = dto.CustomDepositAmount,
                Status = TableStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            var result = await GetTableByIdAsync(table.Id);

            _logger.LogInformation("Table created: {TableNumber}", table.TableNumber);
            return (true, "Tạo bàn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table");
            return (false, "Đã xảy ra lỗi khi tạo bàn", null);
        }
    }

    public async Task<(bool Success, string Message, List<TableDto>? Data)> BulkCreateTablesAsync(BulkCreateTablesDto dto)
    {
        try
        {
            // Check table type exists
            if (!await _context.TableTypes.AnyAsync(tt => tt.Id == dto.TableTypeId))
            {
                return (false, "Loại bàn không tồn tại", null);
            }

            // Find or create zone
            var zone = await _context.Zones.FirstOrDefaultAsync(z => z.Name == dto.ZoneName);
            if (zone == null)
            {
                zone = new Zone
                {
                    Name = dto.ZoneName,
                    Position = await _context.Zones.CountAsync() + 1,
                    IsActive = true
                };
                _context.Zones.Add(zone);
                await _context.SaveChangesAsync();
            }

            var createdTables = new List<Table>();

            for (int i = 1; i <= dto.Quantity; i++)
            {
                var tableNumber = $"{dto.TablePrefix}{i}";

                // Skip if exists
                if (await _context.Tables.AnyAsync(t => t.TableNumber == tableNumber))
                {
                    _logger.LogWarning("Table {TableNumber} already exists, skipping", tableNumber);
                    continue;
                }

                var table = new Table
                {
                    ZoneId = zone.Id,
                    TableTypeId = dto.TableTypeId,
                    TableNumber = tableNumber,
                    Capacity = dto.Capacity,
                    Status = TableStatus.Available,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tables.Add(table);
                createdTables.Add(table);
            }

            await _context.SaveChangesAsync();

            var result = new List<TableDto>();
            foreach (var table in createdTables)
            {
                var tableDto = await GetTableByIdAsync(table.Id);
                if (tableDto != null)
                {
                    result.Add(tableDto);
                }
            }

            _logger.LogInformation("Bulk created {Count} tables in zone {ZoneName}", createdTables.Count, dto.ZoneName);
            return (true, $"Tạo thành công {createdTables.Count} bàn", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating tables");
            return (false, "Đã xảy ra lỗi khi tạo bàn hàng loạt", null);
        }
    }

    public async Task<(bool Success, string Message, TableDto? Data)> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        try
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return (false, "Không tìm thấy bàn", null);
            }

            // Check table type exists
            if (!await _context.TableTypes.AnyAsync(tt => tt.Id == dto.TableTypeId))
            {
                return (false, "Loại bàn không tồn tại", null);
            }

            // Check zone exists (if provided)
            if (dto.ZoneId.HasValue && !await _context.Zones.AnyAsync(z => z.Id == dto.ZoneId.Value))
            {
                return (false, "Khu vực không tồn tại", null);
            }

            // Check duplicate table number (exclude current)
            if (await _context.Tables.AnyAsync(t => t.TableNumber == dto.TableNumber && t.Id != id))
            {
                return (false, "Số bàn đã tồn tại", null);
            }

            table.ZoneId = dto.ZoneId;
            table.TableTypeId = dto.TableTypeId;
            table.TableNumber = dto.TableNumber;
            table.Capacity = dto.Capacity;
            table.CustomDepositAmount = dto.CustomDepositAmount;
            table.IsActive = dto.IsActive;
            table.LastMaintenanceDate = dto.LastMaintenanceDate;
            table.MaintenanceNote = dto.MaintenanceNote;

            await _context.SaveChangesAsync();

            var result = await GetTableByIdAsync(id);

            _logger.LogInformation("Table updated: {TableId}", id);
            return (true, "Cập nhật bàn thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table {TableId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật bàn", null);
        }
    }

    public async Task<(bool Success, string Message)> DeleteTableAsync(int id)
    {
        try
        {
            var table = await _context.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return (false, "Không tìm thấy bàn");
            }

            // Check if table has bookings
            if (table.Bookings.Any(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed))
            {
                return (false, "Không thể xóa bàn có booking đang hoạt động");
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Table deleted: {TableId}", id);
            return (true, "Xóa bàn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting table {TableId}", id);
            return (false, "Đã xảy ra lỗi khi xóa bàn");
        }
    }

    public async Task<(bool Success, string Message)> UpdateTableStatusAsync(int id, UpdateTableStatusDto dto)
    {
        try
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return (false, "Không tìm thấy bàn");
            }

            table.Status = dto.Status;

            if (dto.Status == TableStatus.Maintenance)
            {
                table.LastMaintenanceDate = DateTime.UtcNow;
                table.MaintenanceNote = dto.Note;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Table status updated: {TableId} -> {Status}", id, dto.Status);
            return (true, $"Cập nhật trạng thái bàn thành công: {dto.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table status {TableId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật trạng thái bàn");
        }
    }
}