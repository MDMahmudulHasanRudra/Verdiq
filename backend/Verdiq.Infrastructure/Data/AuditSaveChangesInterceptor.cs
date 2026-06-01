using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        var ipAddress = GetIpAddress();
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.UpdatedAt = now;
            }

            if (userId.HasValue)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId.Value,
                    Action = entry.State.ToString(),
                    Entity = entry.Entity.GetType().Name,
                    EntityId = entity.Id.ToString(),
                    IpAddress = ipAddress,
                    CreatedAt = now
                };

                context.Set<AuditLog>().Add(auditLog);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim is not null ? Guid.Parse(userIdClaim) : null;
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
