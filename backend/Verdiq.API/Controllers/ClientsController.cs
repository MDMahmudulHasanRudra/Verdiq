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
    public async Task<ActionResult<PagedResponse<ClientResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var clients = role == "Admin"
            ? await _clientService.GetAllClientsAsync()
            : await _clientService.GetAllClientsAsync(userId);

        if (!string.IsNullOrWhiteSpace(search))
            clients = clients.Where(c =>
                c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);
            clients = clients.Where(c => c.IsActive == isActive);
        }

        clients = sortBy.ToLower() switch
        {
            "name" => sortOrder == "asc" ? clients.OrderBy(c => c.FullName) : clients.OrderByDescending(c => c.FullName),
            "casescount" => sortOrder == "asc" ? clients.OrderBy(c => c.CasesCount) : clients.OrderByDescending(c => c.CasesCount),
            _ => sortOrder == "asc" ? clients.OrderBy(c => c.JoinedDate) : clients.OrderByDescending(c => c.JoinedDate),
        };

        var totalCount = clients.Count();
        var paged = clients.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PagedResponse<ClientResponseDto>
        {
            Data = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> GetById(Guid id)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(id);
            return Ok(ApiResponse<ClientResponseDto>.Ok(client));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClientResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Create([FromBody] CreateClientDto dto)
    {
        try
        {
            var userId = GetUserId();
            var client = await _clientService.CreateClientAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = client.Id },
                ApiResponse<ClientResponseDto>.Created(client));
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(ApiResponse<ClientResponseDto>.Fail(message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Update(Guid id, [FromBody] UpdateClientDto dto)
    {
        try
        {
            var client = await _clientService.UpdateClientAsync(id, dto);
            return Ok(ApiResponse<ClientResponseDto>.Ok(client));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClientResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _clientService.DeleteClientAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Client deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<ClientResponseDto>>>> Search([FromQuery] string q)
    {
        var userId = GetUserId();
        var clients = await _clientService.SearchClientsAsync(q, userId);
        return Ok(ApiResponse<List<ClientResponseDto>>.Ok(clients.ToList()));
    }
}
