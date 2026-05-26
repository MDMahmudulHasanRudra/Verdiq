using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Chamber;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/chambers")]
public class ChambersController : BaseController
{
    private readonly IChamberService _chamberService;

    public ChambersController(IChamberService chamberService)
    {
        _chamberService = chamberService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChamberResponseDto>>> Create([FromBody] CreateChamberDto dto)
    {
        var (success, message, data) = await _chamberService.CreateAsync(dto, GetUserId());

        if (!success)
            return BadRequest(ApiResponse<ChamberResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<ChamberResponseDto>.Created(data));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ChamberResponseDto>>> GetById(Guid id)
    {
        var chamber = await _chamberService.GetByIdAsync(id);

        if (chamber is null)
            return NotFound(ApiResponse<ChamberResponseDto>.Fail("Chamber not found"));

        return Ok(ApiResponse<ChamberResponseDto>.Ok(chamber));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChamberResponseDto>>> Update(Guid id, [FromBody] UpdateChamberDto dto)
    {
        var (success, message, data) = await _chamberService.UpdateAsync(id, dto);

        if (!success)
            return NotFound(ApiResponse<ChamberResponseDto>.Fail(message));

        return Ok(ApiResponse<ChamberResponseDto>.Ok(data!));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChamberResponseDto>>>> GetAll()
    {
        var chambers = await _chamberService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ChamberResponseDto>>.Ok(chambers));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChamberResponseDto>>> GetMyChamber()
    {
        var chambers = await _chamberService.GetAllAsync();
        var chamber = chambers.FirstOrDefault();

        if (chamber is null)
            return NotFound(ApiResponse<ChamberResponseDto>.Fail("No chamber found"));

        return Ok(ApiResponse<ChamberResponseDto>.Ok(chamber));
    }
}
