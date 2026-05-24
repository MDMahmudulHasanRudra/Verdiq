using Verdiq.Application.DTOs.Subscription;

namespace Verdiq.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto> GetUserSubscriptionAsync(Guid userId);
    Task<SubscriptionResponseDto> ChangePlanAsync(Guid userId, string plan);
    Task CancelSubscriptionAsync(Guid userId);
    Task<IEnumerable<SubscriptionResponseDto>> GetAllSubscriptionsAsync();
}
