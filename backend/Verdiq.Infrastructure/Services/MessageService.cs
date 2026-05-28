using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.ClientPortal;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MessageDto>> GetConversationAsync(Guid userId1, Guid userId2, Guid? caseId = null)
    {
        var query = _context.Messages
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1));

        if (caseId.HasValue)
            query = query.Where(m => m.CaseId == caseId.Value);

        return await query
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                SenderAvatar = m.Sender.AvatarUrl,
                Content = m.Content,
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<MessageDto>> GetClientMessagesAsync(Guid clientUserId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == clientUserId || m.SenderId == clientUserId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                SenderAvatar = m.Sender.AvatarUrl,
                Content = m.Content,
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            CaseId = dto.CaseId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var sender = await _context.Users.FindAsync(senderId);

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? "",
            SenderAvatar = sender?.AvatarUrl,
            Content = message.Content,
            IsRead = false,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<bool> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

        if (message == null) return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }
}
