using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Subscription;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;

    public SubscriptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionResponseDto?> GetByChamberIdAsync(Guid chamberId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Chamber)
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        return subscription == null ? null : MapToDto(subscription);
    }

    public async Task<(bool Success, string Message)> ChangePlanAsync(Guid chamberId, string plan)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        if (subscription == null)
            return (false, "Subscription not found for this chamber");

        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var newPlan))
            return (false, "Invalid plan. Valid values: Free, Pro, Chamber");

        subscription.Plan = newPlan;
        subscription.UpdatedAt = DateTime.UtcNow;

        if (newPlan == SubscriptionPlan.Free)
            subscription.Status = SubscriptionStatus.Active;

        await _context.SaveChangesAsync();
        return (true, $"Plan changed to {newPlan} successfully");
    }

    public async Task<(bool Success, string Message)> CancelAsync(Guid chamberId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        if (subscription == null)
            return (false, "Subscription not found for this chamber");

        subscription.CancelAtPeriodEnd = true;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Subscription will be cancelled at the end of the current billing period");
    }

    public async Task<IEnumerable<SubscriptionResponseDto>> GetAllAsync()
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Chamber)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return subscriptions.Select(MapToDto);
    }

    private static SubscriptionResponseDto MapToDto(Domain.Entities.Subscription s) => new()
    {
        Id = s.Id,
        ChamberId = s.ChamberId,
        Plan = s.Plan.ToString(),
        Status = s.Status.ToString(),
        CurrentPeriodStart = s.CurrentPeriodStart,
        CurrentPeriodEnd = s.CurrentPeriodEnd,
        CancelAtPeriodEnd = s.CancelAtPeriodEnd
    };
}
