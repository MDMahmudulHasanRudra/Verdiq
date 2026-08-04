using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.API.Services;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CasesController : BaseController
{
    private readonly ICaseService _caseService;
    private readonly IRealtimeNotifier _notifier;

    public CasesController(ICaseService caseService, IRealtimeNotifier notifier)
    {
        _caseService = caseService;
        _notifier = notifier;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CaseResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? type = null,
        [FromQuery] string? courtName = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var chamberId = GetChamberId();
        var cases = await _caseService.GetAllAsync(chamberId, status, priority, search, sortBy, sortOrder, page, pageSize, type, courtName, dateFrom, dateTo);
        var totalCount = await _caseService.GetCountAsync(chamberId, status, priority, type, courtName, dateFrom, dateTo);

        return Ok(new PagedResponse<CaseResponseDto>
        {
            Data = cases.ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CaseResponseDto>>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(ApiResponse<IEnumerable<CaseResponseDto>>.Ok(Enumerable.Empty<CaseResponseDto>()));

        var chamberId = GetChamberId();
        var results = await _caseService.SearchAsync(q, chamberId);
        return Ok(ApiResponse<IEnumerable<CaseResponseDto>>.Ok(results));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> GetById(Guid id)
    {
        var caseEntity = await _caseService.GetByIdAsync(id);
        if (caseEntity == null)
            return NotFound(ApiResponse<CaseResponseDto>.Fail("Case not found"));

        return Ok(ApiResponse<CaseResponseDto>.Ok(caseEntity));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Create([FromBody] CreateCaseDto dto)
    {
        var (success, message, data) = await _caseService.CreateAsync(dto, GetUserId(), GetChamberId());
        if (!success || data == null)
            return BadRequest(ApiResponse<CaseResponseDto>.Fail(message));

        await _notifier.NotifyCaseGroupAsync(data.Id.ToString(), "CaseUpdated", data);
        await _notifier.NotifyUserAsync(GetUserId().ToString(), "NotificationReceived", new { title = "Case Created", description = $"Case {data.CaseNumber} created" });

        return CreatedAtAction(nameof(GetById), new { id = data.Id },
            ApiResponse<CaseResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Update(Guid id, [FromBody] UpdateCaseDto dto)
    {
        var (success, message, data) = await _caseService.UpdateAsync(id, dto);
        if (!success || data == null)
            return NotFound(ApiResponse<CaseResponseDto>.Fail(message));

        await _notifier.NotifyCaseGroupAsync(id.ToString(), "CaseUpdated", data);

        return Ok(ApiResponse<CaseResponseDto>.Ok(data));
    }

    [HttpPost("bulk-status")]
    public async Task<ActionResult<ApiResponse<object>>> BulkStatusChange([FromBody] BulkStatusChangeDto dto)
    {
        var (successCount, failCount, message) = await _caseService.BulkStatusChangeAsync(dto, GetChamberId());
        return Ok(ApiResponse<object>.Ok(new { successCount, failCount, message }));
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<ApiResponse<object>>> BulkDelete([FromBody] BulkDeleteDto dto)
    {
        var (successCount, failCount, message) = await _caseService.BulkDeleteAsync(dto, GetChamberId());
        return Ok(ApiResponse<object>.Ok(new { successCount, failCount, message }));
    }

    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Duplicate(Guid id)
    {
        var (success, message, data) = await _caseService.DuplicateAsync(id, GetUserId(), GetChamberId());
        if (!success || data == null)
            return BadRequest(ApiResponse<CaseResponseDto>.Fail(message));

        return Ok(ApiResponse<CaseResponseDto>.Ok(data, message));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, [FromBody] ConfirmCaseDeleteDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(ApiResponse<object>.Fail("Email and password are required to delete a case"));

        var (success, message) = await _caseService.DeleteAsync(id, dto.Email, dto.Password);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        await _notifier.NotifyCaseGroupAsync(id.ToString(), "CaseUpdated", new { deleted = true });

        return Ok(ApiResponse<object>.Ok(null!, "Case deleted successfully"));
    }
}
