using Verdiq.Application.DTOs.Notification;

namespace Verdiq.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId);
}
