using Verdiq.Application.DTOs.Subscription;

namespace Verdiq.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto?> GetByChamberIdAsync(Guid chamberId);
    Task<(bool Success, string Message)> ChangePlanAsync(Guid chamberId, string plan);
    Task<(bool Success, string Message)> CancelAsync(Guid chamberId);
    Task<IEnumerable<SubscriptionResponseDto>> GetAllAsync();
}
