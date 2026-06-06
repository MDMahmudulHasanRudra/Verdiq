using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Audit;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : BaseController
{
    private readonly IAuditService _service;
    public AuditController(IAuditService service) => _service = service;

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<AuditSummaryDto>>> GetSummary()
        => Ok(ApiResponse<AuditSummaryDto>.Ok(await _service.GetSummaryAsync(GetChamberId())));

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<List<AuditLogResponseDto>>>> GetLogs(
        [FromQuery] string? entity, [FromQuery] string? action,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(ApiResponse<List<AuditLogResponseDto>>.Ok(await _service.GetLogsAsync(GetChamberId(), entity, action, page, pageSize)));
}
