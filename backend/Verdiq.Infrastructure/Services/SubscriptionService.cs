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

    public async Task<SubscriptionResponseDto> GetUserSubscriptionAsync(Guid userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found");

        return MapToDto(subscription);
    }

    public async Task<SubscriptionResponseDto> ChangePlanAsync(Guid userId, string plan)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found");

        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var newPlan))
            throw new ArgumentException("Invalid plan. Valid values: Free, Pro, Chamber");

        subscription.Plan = newPlan;
        subscription.UpdatedAt = DateTime.UtcNow;

        if (newPlan == SubscriptionPlan.Free)
        {
            subscription.Status = SubscriptionStatus.Active;
        }

        await _context.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task CancelSubscriptionAsync(Guid userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found");

        subscription.CancelAtPeriodEnd = true;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SubscriptionResponseDto>> GetAllSubscriptionsAsync()
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return subscriptions.Select(MapToDto);
    }

    private static SubscriptionResponseDto MapToDto(Domain.Entities.Subscription s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        Plan = s.Plan.ToString(),
        Status = s.Status.ToString(),
        CurrentPeriodStart = s.CurrentPeriodStart,
        CurrentPeriodEnd = s.CurrentPeriodEnd,
        CancelAtPeriodEnd = s.CancelAtPeriodEnd
    };
}
