using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Verdiq.Domain.Entities;

namespace Verdiq.Infrastructure.Data;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = GetCurrentUserId();
        var chamberId = GetCurrentChamberId();
        var userName = GetCurrentUserName();
        var ipAddress = GetIpAddress();
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                e.Entity.GetType() != typeof(AuditLog) &&
                (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            string? oldValues = null;
            string? newValues = null;
            string action;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                action = "Added";
                newValues = SerializeCurrentValues(entry);
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
                action = "Modified";
                oldValues = SerializeOriginalValues(entry);
                newValues = SerializeCurrentValues(entry);
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.UpdatedAt = now;
                action = "Deleted";
                oldValues = SerializeCurrentValues(entry);
            }
            else
            {
                continue;
            }

            if (userId.HasValue)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId.Value,
                    ChamberId = chamberId ?? Guid.Empty,
                    UserName = userName ?? "Unknown",
                    Action = action,
                    Entity = entry.Entity.GetType().Name,
                    EntityId = entity.Id.ToString(),
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    CreatedAt = now
                };

                context.Set<AuditLog>().Add(auditLog);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string? SerializeOriginalValues(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey() || prop.Metadata.IsShadowProperty())
                continue;

            var original = prop.OriginalValue;
            var current = prop.CurrentValue;

            if (!Equals(original, current))
            {
                changes[prop.Metadata.Name] = original;
            }
        }
        return changes.Count == 0 ? null : JsonSerializer.Serialize(changes, _jsonOptions);
    }

    private static string? SerializeCurrentValues(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey() || prop.Metadata.IsShadowProperty())
                continue;

            values[prop.Metadata.Name] = prop.CurrentValue;
        }
        return values.Count == 0 ? null : JsonSerializer.Serialize(values, _jsonOptions);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim is not null ? Guid.Parse(userIdClaim) : null;
    }

    private Guid? GetCurrentChamberId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var chamberClaim = user?.FindFirst("chamberId")?.Value;
        return chamberClaim is not null ? Guid.Parse(chamberClaim) : null;
    }

    private string? GetCurrentUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.Name)?.Value
            ?? user?.FindFirst("name")?.Value;
    }

    private string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return "unknown";

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
