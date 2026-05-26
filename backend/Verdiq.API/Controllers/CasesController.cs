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
    public async Task<ActionResult<PagedResponse<CaseResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        var chamberId = GetChamberId();
        var cases = await _caseService.GetAllAsync(chamberId, status, priority, page, pageSize);
        var totalCount = await _caseService.GetCountAsync(chamberId);

        return Ok(new PagedResponse<CaseResponseDto>
        {
            Data = cases.ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
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

        return CreatedAtAction(nameof(GetById), new { id = data.Id },
            ApiResponse<CaseResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CaseResponseDto>>> Update(Guid id, [FromBody] UpdateCaseDto dto)
    {
        var (success, message, data) = await _caseService.UpdateAsync(id, dto);
        if (!success || data == null)
            return NotFound(ApiResponse<CaseResponseDto>.Fail(message));

        return Ok(ApiResponse<CaseResponseDto>.Ok(data));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _caseService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, "Case deleted successfully"));
    }
}
