using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Audit;
using Verdiq.Application.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context) => _context = context;

    public async Task<AuditSummaryDto> GetSummaryAsync(Guid chamberId)
    {
        var query = _context.AuditLogs.Where(l => l.ChamberId == chamberId);

        var totalCount = await query.CountAsync();
        var todayCount = await query.CountAsync(l => l.CreatedAt.Date == DateTime.UtcNow.Date);

        var byEntity = await query
            .GroupBy(l => l.Entity)
            .Select(g => new { Entity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Entity, g => g.Count);

        var byAction = await query
            .GroupBy(l => l.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Action, g => g.Count);

        var recentLogs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .Select(l => MapLog(l))
            .ToListAsync();

        return new AuditSummaryDto
        {
            TotalActions = totalCount,
            TodayActions = todayCount,
            ByEntity = byEntity,
            ByAction = byAction,
            RecentLogs = recentLogs
        };
    }

    public async Task<(List<AuditLogResponseDto> Items, int TotalCount)> GetLogsAsync(Guid chamberId, AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs
            .Where(l => l.ChamberId == chamberId);

        if (!string.IsNullOrEmpty(filter.Entity))
            query = query.Where(l => l.Entity == filter.Entity);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(l => l.Action == filter.Action);

        if (filter.UserId.HasValue)
            query = query.Where(l => l.UserId == filter.UserId.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= filter.DateTo.Value);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(l =>
                l.Entity.ToLower().Contains(search) ||
                l.Action.ToLower().Contains(search) ||
                l.UserName.ToLower().Contains(search) ||
                (l.OldValues != null && l.OldValues.ToLower().Contains(search)) ||
                (l.NewValues != null && l.NewValues.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => MapLog(l))
            .ToListAsync();

        return (logs, totalCount);
    }

    private static AuditLogResponseDto MapLog(Domain.Entities.AuditLog l)
    {
        var dto = new AuditLogResponseDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.UserName,
            ChamberId = l.ChamberId,
            Action = l.Action,
            Entity = l.Entity,
            EntityId = l.EntityId ?? "",
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            IpAddress = l.IpAddress,
            CreatedAt = l.CreatedAt,
            Changes = ParseChanges(l)
        };
        return dto;
    }

    private static List<AuditFieldChangeDto> ParseChanges(Domain.Entities.AuditLog l)
    {
        var changes = new List<AuditFieldChangeDto>();
        if (string.IsNullOrEmpty(l.OldValues) && string.IsNullOrEmpty(l.NewValues))
            return changes;

        var old = string.IsNullOrEmpty(l.OldValues)
            ? new Dictionary<string, object?>()
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(l.OldValues) ?? new();

        var @new = string.IsNullOrEmpty(l.NewValues)
            ? new Dictionary<string, object?>()
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(l.NewValues) ?? new();

        var allKeys = old.Keys.Union(@new.Keys).Distinct().ToList();
        foreach (var key in allKeys)
        {
            old.TryGetValue(key, out var oldVal);
            @new.TryGetValue(key, out var newVal);
            var oldStr = oldVal?.ToString();
            var newStr = newVal?.ToString();
            if (oldStr != newStr)
            {
                changes.Add(new AuditFieldChangeDto
                {
                    Field = key,
                    OldValue = oldStr,
                    NewValue = newStr
                });
            }
        }
        return changes;
    }
}
