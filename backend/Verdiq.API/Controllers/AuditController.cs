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
    public async Task<ActionResult<ApiResponse<object>>> GetLogs(
        [FromQuery] string? entity,
        [FromQuery] string? action,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var filter = new AuditLogFilterDto
        {
            Entity = entity,
            Action = action,
            UserId = userId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var (items, totalCount) = await _service.GetLogsAsync(GetChamberId(), filter);
        return Ok(new
        {
            success = true,
            data = items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}
