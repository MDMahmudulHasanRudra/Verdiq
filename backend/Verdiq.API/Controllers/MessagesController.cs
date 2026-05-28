using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.ClientPortal;
using Verdiq.Application.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : BaseController
{
    private readonly IMessageService _messageService;
    private readonly AppDbContext _context;

    public MessagesController(IMessageService messageService, AppDbContext context)
    {
        _messageService = messageService;
        _context = context;
    }

    [HttpGet("conversation/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetConversation(
        Guid otherUserId, [FromQuery] Guid? caseId = null)
    {
        var userId = GetUserId();
        var data = await _messageService.GetConversationAsync(userId, otherUserId, caseId);
        return Ok(ApiResponse<IEnumerable<MessageDto>>.Ok(data));
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetClientConversation(Guid clientId)
    {
        var userId = GetUserId();
        var client = await _context.Clients.FindAsync(clientId);
        if (client?.UserId == null)
            return NotFound(ApiResponse<IEnumerable<MessageDto>>.Fail("Client has no portal account"));

        var data = await _messageService.GetConversationAsync(userId, client.UserId.Value);
        return Ok(ApiResponse<IEnumerable<MessageDto>>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var data = await _messageService.SendMessageAsync(userId, dto);
        return Ok(ApiResponse<MessageDto>.Ok(data));
    }

    [HttpPost("{messageId}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid messageId)
    {
        var userId = GetUserId();
        var success = await _messageService.MarkAsReadAsync(messageId, userId);
        return Ok(ApiResponse<bool>.Ok(success));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }
}
