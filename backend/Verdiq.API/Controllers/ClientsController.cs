using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.DTOs.ClientPortal;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : BaseController
{
    private readonly IClientService _clientService;
    private readonly AppDbContext _context;

    public ClientsController(IClientService clientService, AppDbContext context)
    {
        _clientService = clientService;
        _context = context;
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

    [HttpPost("{clientId}/portal-access")]
    public async Task<ActionResult<ApiResponse<object>>> CreatePortalAccess(Guid clientId, [FromBody] ClientRegisterDto dto)
    {
        var chamberId = GetChamberId();

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.ChamberId == chamberId);
        if (client == null)
            return NotFound(ApiResponse<object>.Fail("Client not found"));

        if (client.UserId.HasValue)
            return BadRequest(ApiResponse<object>.Fail("Client already has portal access"));

        var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (existingUser)
            return BadRequest(ApiResponse<object>.Fail("Email already in use"));

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            Role = UserRole.Client,
            ChamberId = chamberId,
            ClientId = clientId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        client.UserId = user.Id;
        client.Email = dto.Email;
        client.Phone = dto.Phone;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { userId = user.Id, clientId = client.Id }, "Portal access created"));
    }

    [HttpPost("{clientId}/revoke-portal")]
    public async Task<ActionResult<ApiResponse<object>>> RevokePortalAccess(Guid clientId)
    {
        var chamberId = GetChamberId();

        var client = await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == clientId && c.ChamberId == chamberId);

        if (client == null)
            return NotFound(ApiResponse<object>.Fail("Client not found"));

        if (client.User == null)
            return BadRequest(ApiResponse<object>.Fail("Client has no portal access"));

        client.User.IsActive = false;
        client.User.ClientId = null;
        client.UserId = null;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Portal access revoked"));
    }
}
