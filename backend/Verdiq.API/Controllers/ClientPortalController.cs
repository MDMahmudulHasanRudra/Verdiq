using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.ClientPortal;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/client-portal")]
[Authorize(Roles = "Client")]
public class ClientPortalController : BaseController
{
    private readonly IClientPortalService _portalService;
    private readonly IMessageService _messageService;
    private readonly AppDbContext _context;

    public ClientPortalController(IClientPortalService portalService, IMessageService messageService, AppDbContext context)
    {
        _portalService = portalService;
        _messageService = messageService;
        _context = context;
    }

    private async Task<Guid?> GetClientIdAsync()
    {
        var userId = GetUserId();
        var user = await _context.Users.Include(u => u.Client).FirstOrDefaultAsync(u => u.Id == userId);
        return user?.ClientId;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<ClientDashboardDto>>> GetDashboard()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<ClientDashboardDto>.Fail("Client profile not linked"));
        var data = await _portalService.GetDashboardAsync(clientId.Value);
        return Ok(ApiResponse<ClientDashboardDto>.Ok(data));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<ClientProfileDto>>> GetProfile()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<ClientProfileDto>.Fail("Client profile not linked"));
        var data = await _portalService.GetProfileAsync(clientId.Value);
        return Ok(ApiResponse<ClientProfileDto>.Ok(data));
    }

    [HttpGet("cases")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientCaseSummaryDto>>>> GetCases()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<IEnumerable<ClientCaseSummaryDto>>.Fail("Client profile not linked"));
        var data = await _portalService.GetCasesAsync(clientId.Value);
        return Ok(ApiResponse<IEnumerable<ClientCaseSummaryDto>>.Ok(data));
    }

    [HttpGet("cases/{caseId}")]
    public async Task<ActionResult<ApiResponse<ClientCaseDetailDto>>> GetCaseDetail(Guid caseId)
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<ClientCaseDetailDto>.Fail("Client profile not linked"));
        var data = await _portalService.GetCaseDetailAsync(caseId, clientId.Value);
        if (data == null)
            return NotFound(ApiResponse<ClientCaseDetailDto>.Fail("Case not found"));
        return Ok(ApiResponse<ClientCaseDetailDto>.Ok(data));
    }

    [HttpGet("hearings")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientHearingDto>>>> GetUpcomingHearings()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<IEnumerable<ClientHearingDto>>.Fail("Client profile not linked"));
        var data = await _portalService.GetUpcomingHearingsAsync(clientId.Value);
        return Ok(ApiResponse<IEnumerable<ClientHearingDto>>.Ok(data));
    }

    [HttpGet("documents")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientDocumentDto>>>> GetDocuments()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<IEnumerable<ClientDocumentDto>>.Fail("Client profile not linked"));
        var data = await _portalService.GetDocumentsAsync(clientId.Value);
        return Ok(ApiResponse<IEnumerable<ClientDocumentDto>>.Ok(data));
    }

    [HttpGet("documents/{id}")]
    public async Task<ActionResult<ApiResponse<ClientDocumentDto>>> GetDocument(Guid id)
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<ClientDocumentDto>.Fail("Client profile not linked"));
        var data = await _portalService.GetDocumentByIdAsync(id, clientId.Value);
        if (data == null)
            return NotFound(ApiResponse<ClientDocumentDto>.Fail("Document not found"));
        return Ok(ApiResponse<ClientDocumentDto>.Ok(data));
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientInvoiceDto>>>> GetInvoices()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<IEnumerable<ClientInvoiceDto>>.Fail("Client profile not linked"));
        var data = await _portalService.GetInvoicesAsync(clientId.Value);
        return Ok(ApiResponse<IEnumerable<ClientInvoiceDto>>.Ok(data));
    }

    [HttpGet("tasks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientTaskDto>>>> GetTasks()
    {
        var clientId = await GetClientIdAsync();
        if (clientId == null)
            return Unauthorized(ApiResponse<IEnumerable<ClientTaskDto>>.Fail("Client profile not linked"));
        var data = await _portalService.GetTasksAsync(clientId.Value);
        return Ok(ApiResponse<IEnumerable<ClientTaskDto>>.Ok(data));
    }

    [HttpPost("messages")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var data = await _messageService.SendMessageAsync(userId, dto);
        return Ok(ApiResponse<MessageDto>.Ok(data));
    }

    [HttpGet("messages")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetMessages()
    {
        var userId = GetUserId();
        var data = await _messageService.GetClientMessagesAsync(userId);
        return Ok(ApiResponse<IEnumerable<MessageDto>>.Ok(data));
    }

    [HttpGet("messages/unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid messageId)
    {
        var userId = GetUserId();
        var success = await _messageService.MarkAsReadAsync(messageId, userId);
        return Ok(ApiResponse<bool>.Ok(success));
    }
}
