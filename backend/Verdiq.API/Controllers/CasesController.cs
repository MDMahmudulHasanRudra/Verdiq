using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CasesController : BaseController
{
    private readonly ICaseService _caseService;

    public CasesController(ICaseService caseService)
    {
        _caseService = caseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CaseResponseDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var cases = role == "Admin"
            ? await _caseService.GetAllCasesAsync()
            : await _caseService.GetAllCasesAsync(userId);

        if (!string.IsNullOrWhiteSpace(search))
            cases = cases.Where(c =>
                c.CaseNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.ClientName.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(status))
            cases = cases.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(type))
            cases = cases.Where(c => c.CaseType.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(priority))
            cases = cases.Where(c => c.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));

        if (clientId.HasValue)
            cases = cases.Where(c => c.ClientId == clientId.Value);

        cases = sortBy.ToLower() switch
        {
            "casenumber" => sortOrder == "asc" ? cases.OrderBy(c => c.CaseNumber) : cases.OrderByDescending(c => c.CaseNumber),
            "title" => sortOrder == "asc" ? cases.OrderBy(c => c.Title) : cases.OrderByDescending(c => c.Title),
            "status" => sortOrder == "asc" ? cases.OrderBy(c => c.Status) : cases.OrderByDescending(c => c.Status),
            "priority" => sortOrder == "asc" ? cases.OrderBy(c => c.Priority) : cases.OrderByDescending(c => c.Priority),
            "filingdate" => sortOrder == "asc" ? cases.OrderBy(c => c.FilingDate) : cases.OrderByDescending(c => c.FilingDate),
            _ => sortOrder == "asc" ? cases.OrderBy(c => c.CreatedAt) : cases.OrderByDescending(c => c.CreatedAt),
        };

        var totalCount = cases.Count();
        var paged = cases.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PagedResponse<CaseResponseDto>
        {
            Data = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> GetById(Guid id)
    {
        try
        {
            var caseEntity = await _caseService.GetCaseByIdAsync(id);
            return Ok(ApiResponse<CaseResponseDto>.Ok(caseEntity));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CaseResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Create([FromBody] CreateCaseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var caseEntity = await _caseService.CreateCaseAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = caseEntity.Id },
                ApiResponse<CaseResponseDto>.Created(caseEntity));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ApiResponse<CaseResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Update(Guid id, [FromBody] UpdateCaseDto dto)
    {
        try
        {
            var caseEntity = await _caseService.UpdateCaseAsync(id, dto);
            return Ok(ApiResponse<CaseResponseDto>.Ok(caseEntity));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CaseResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _caseService.DeleteCaseAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Case deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<CaseResponseDto>>>> Search([FromQuery] string q)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var cases = role == "Admin"
            ? await _caseService.SearchCasesAsync(q)
            : await _caseService.SearchCasesAsync(q, userId);

        return Ok(ApiResponse<List<CaseResponseDto>>.Ok(cases.ToList()));
    }
}
