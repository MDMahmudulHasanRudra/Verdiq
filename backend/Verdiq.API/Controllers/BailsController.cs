using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Bail;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BailsController : BaseController
{
    private readonly IBailService _bailService;

    public BailsController(IBailService bailService) => _bailService = bailService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BailResponseDto>>>> GetAll([FromQuery] string? status)
    {
        var chamberId = GetChamberId();
        var bails = await _bailService.GetAllAsync(chamberId, status);
        return Ok(ApiResponse<IEnumerable<BailResponseDto>>.Ok(bails));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BailResponseDto>>> GetById(Guid id)
    {
        var bail = await _bailService.GetByIdAsync(id);
        if (bail == null)
            return NotFound(ApiResponse<BailResponseDto>.Fail("Bail record not found"));

        return Ok(ApiResponse<BailResponseDto>.Ok(bail));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<BailResponseDto>>> GetByCase(Guid caseId)
    {
        var bail = await _bailService.GetByCaseIdAsync(caseId);
        if (bail == null)
            return Ok(ApiResponse<BailResponseDto>.Ok(null!, "No bail record for this case"));

        return Ok(ApiResponse<BailResponseDto>.Ok(bail));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BailResponseDto>>> Create([FromBody] CreateBailDto dto)
    {
        var (success, message, data) = await _bailService.CreateAsync(dto, GetChamberId());
        if (!success || data == null)
            return BadRequest(ApiResponse<BailResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data.Id },
            ApiResponse<BailResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BailResponseDto>>> Update(Guid id, [FromBody] UpdateBailDto dto)
    {
        var (success, message, data) = await _bailService.UpdateAsync(id, dto);
        if (!success || data == null)
            return NotFound(ApiResponse<BailResponseDto>.Fail(message));

        return Ok(ApiResponse<BailResponseDto>.Ok(data));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<BailResponseDto>>> UpdateStatus(Guid id, [FromBody] UpdateBailStatusDto dto)
    {
        var (success, message, data) = await _bailService.UpdateStatusAsync(id, dto);
        if (!success || data == null)
            return BadRequest(ApiResponse<BailResponseDto>.Fail(message));

        return Ok(ApiResponse<BailResponseDto>.Ok(data));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _bailService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
