using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Audit;
using Verdiq.Application.Interfaces;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    public AuditService(AppDbContext context) => _context = context;

    public async Task<AuditSummaryDto> GetSummaryAsync(Guid chamberId)
    {
        var allLogs = await _context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var chamberLogs = allLogs.Where(l => true).ToList();
        var today = DateTime.UtcNow.Date;

        return new AuditSummaryDto
        {
            TotalActions = chamberLogs.Count,
            TodayActions = chamberLogs.Count(l => l.CreatedAt.Date == today),
            ByEntity = chamberLogs.GroupBy(l => l.Entity)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByAction = chamberLogs.GroupBy(l => l.Action)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentLogs = chamberLogs.Take(20).Select(MapLog).ToList()
        };
    }

    public async Task<List<AuditLogResponseDto>> GetLogsAsync(Guid chamberId, string? entity, string? action, int page, int pageSize)
    {
        var q = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(entity)) q = q.Where(l => l.Entity == entity);
        if (!string.IsNullOrEmpty(action)) q = q.Where(l => l.Action == action);
        return await q.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapLog(l)).ToListAsync();
    }

    private static AuditLogResponseDto MapLog(Domain.Entities.AuditLog l) => new()
    {
        Id = l.Id, Action = l.Action, Entity = l.Entity,
        EntityId = l.EntityId ?? "", OldValues = l.OldValues,
        NewValues = l.NewValues, IpAddress = l.IpAddress,
        UserId = l.UserId, CreatedAt = l.CreatedAt
    };
}
