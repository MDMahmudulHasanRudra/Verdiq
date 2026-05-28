using Verdiq.Application.DTOs.ClientPortal;

namespace Verdiq.Application.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetConversationAsync(Guid userId1, Guid userId2, Guid? caseId = null);
    Task<IEnumerable<MessageDto>> GetClientMessagesAsync(Guid clientUserId);
    Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto);
    Task<bool> MarkAsReadAsync(Guid messageId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
