using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : BaseController
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ClientResponseDto>>> GetAll(int page = 1, int pageSize = 10)
    {
        var chamberId = GetChamberId();
        var clients = await _clientService.GetAllAsync(chamberId, page, pageSize);
        var totalCount = await _clientService.GetCountAsync(chamberId);
        return Ok(new PagedResponse<ClientResponseDto>
        {
            Data = clients.ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null)
            return NotFound(ApiResponse<ClientResponseDto>.Fail("Client not found"));
        return Ok(ApiResponse<ClientResponseDto>.Ok(client));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Create([FromBody] CreateClientDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _clientService.CreateAsync(dto, chamberId);
        if (!success)
            return BadRequest(ApiResponse<ClientResponseDto>.Fail(message));
        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<ClientResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Update(Guid id, [FromBody] UpdateClientDto dto)
    {
        var (success, message, data) = await _clientService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(ApiResponse<ClientResponseDto>.Fail(message));
        return Ok(ApiResponse<ClientResponseDto>.Ok(data!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _clientService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<ClientResponseDto>>>> Search([FromQuery] string q)
    {
        var chamberId = GetChamberId();
        var clients = await _clientService.SearchAsync(q, chamberId);
        return Ok(ApiResponse<List<ClientResponseDto>>.Ok(clients.ToList()));
    }
}
