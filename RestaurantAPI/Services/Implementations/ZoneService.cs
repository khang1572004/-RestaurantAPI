using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Zone;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services.Implementations;

public class ZoneService : IZoneService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ZoneService> _logger;

    public ZoneService(ApplicationDbContext context, ILogger<ZoneService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ZoneDto>> GetAllZonesAsync(bool includeInactive = false)
    {
        var query = _context.Zones
            .Include(z => z.Tables)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(z => z.IsActive);
        }

        var zones = await query
            .OrderBy(z => z.Position)
            .ThenBy(z => z.Name)
            .Select(z => new ZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                Description = z.Description,
                Position = z.Position,
                IsActive = z.IsActive,
                TablesCount = z.Tables.Count
            })
            .ToListAsync();

        return zones;
    }

    public async Task<ZoneDto?> GetZoneByIdAsync(int id)
    {
        var zone = await _context.Zones
            .Include(z => z.Tables)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zone == null)
            return null;

        return new ZoneDto
        {
            Id = zone.Id,
            Name = zone.Name,
            Description = zone.Description,
            Position = zone.Position,
            IsActive = zone.IsActive,
            TablesCount = zone.Tables.Count
        };
    }

    public async Task<(bool Success, string Message, ZoneDto? Data)> CreateZoneAsync(CreateZoneDto dto)
    {
        try
        {
            if (await _context.Zones.AnyAsync(z => z.Name == dto.Name))
            {
                return (false, "Tên khu vực đã tồn tại", null);
            }

            var zone = new Zone
            {
                Name = dto.Name,
                Description = dto.Description,
                Position = dto.Position,
                IsActive = true
            };

            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            var result = await GetZoneByIdAsync(zone.Id);

            _logger.LogInformation("Zone created: {ZoneName}", zone.Name);
            return (true, "Tạo khu vực thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating zone");
            return (false, "Đã xảy ra lỗi khi tạo khu vực", null);
        }
    }

    public async Task<(bool Success, string Message, ZoneDto? Data)> UpdateZoneAsync(int id, UpdateZoneDto dto)
    {
        try
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
            {
                return (false, "Không tìm thấy khu vực", null);
            }

            if (await _context.Zones.AnyAsync(z => z.Name == dto.Name && z.Id != id))
            {
                return (false, "Tên khu vực đã tồn tại", null);
            }

            zone.Name = dto.Name;
            zone.Description = dto.Description;
            zone.Position = dto.Position;
            zone.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var result = await GetZoneByIdAsync(id);

            _logger.LogInformation("Zone updated: {ZoneId}", id);
            return (true, "Cập nhật khu vực thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating zone {ZoneId}", id);
            return (false, "Đã xảy ra lỗi khi cập nhật khu vực", null);
        }
    }

    public async Task<(bool Success, string Message)> DeleteZoneAsync(int id)
    {
        try
        {
            var zone = await _context.Zones
                .Include(z => z.Tables)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zone == null)
            {
                return (false, "Không tìm thấy khu vực");
            }

            if (zone.Tables.Any())
            {
                return (false, "Không thể xóa khu vực có bàn. Vui lòng xóa các bàn trước.");
            }

            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Zone deleted: {ZoneId}", id);
            return (true, "Xóa khu vực thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting zone {ZoneId}", id);
            return (false, "Đã xảy ra lỗi khi xóa khu vực");
        }
    }
}