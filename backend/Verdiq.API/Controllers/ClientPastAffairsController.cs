using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/past-affairs")]
[Authorize]
public class ClientPastAffairsController : BaseController
{
    private readonly IClientPastAffairService _service;

    public ClientPastAffairsController(IClientPastAffairService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientPastAffairResponseDto>>>> GetAll(Guid clientId)
    {
        var result = await _service.GetByClientIdAsync(clientId);
        return Ok(ApiResponse<IEnumerable<ClientPastAffairResponseDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ClientPastAffairResponseDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<ClientPastAffairResponseDto>.Fail("Record not found"));
        return Ok(ApiResponse<ClientPastAffairResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientPastAffairResponseDto>>> Create(Guid clientId, [FromBody] CreateClientPastAffairDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(clientId, dto, GetChamberId());
        if (!success || data == null)
            return BadRequest(ApiResponse<ClientPastAffairResponseDto>.Fail(message));
        return CreatedAtAction(nameof(GetById), new { clientId, id = data.Id },
            ApiResponse<ClientPastAffairResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ClientPastAffairResponseDto>>> Update(Guid id, [FromBody] UpdateClientPastAffairDto dto)
    {
        var (success, message, data) = await _service.UpdateAsync(id, dto);
        if (!success || data == null)
            return NotFound(ApiResponse<ClientPastAffairResponseDto>.Fail(message));
        return Ok(ApiResponse<ClientPastAffairResponseDto>.Ok(data));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
