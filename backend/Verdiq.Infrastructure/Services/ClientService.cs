using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, ClientResponseDto? Data)> CreateAsync(CreateClientDto dto, Guid chamberId)
    {
        var client = new Client
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            Nid = dto.Nid,
            CompanyName = dto.CompanyName,
            Notes = dto.Notes,
            IsActive = true,
            ChamberId = chamberId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(client.Id);
        return (true, "Client created successfully", result);
    }

    public async Task<(bool Success, string Message, ClientResponseDto? Data)> UpdateAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            return (false, "Client not found", null);

        if (dto.Name != null) client.Name = dto.Name;
        if (dto.Email != null) client.Email = dto.Email;
        if (dto.Phone != null) client.Phone = dto.Phone;
        if (dto.Address != null) client.Address = dto.Address;
        if (dto.Nid != null) client.Nid = dto.Nid;
        if (dto.CompanyName != null) client.CompanyName = dto.CompanyName;
        if (dto.Notes != null) client.Notes = dto.Notes;

        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Client updated successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            return (false, "Client not found");

        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Client deleted successfully");
    }

    public async Task<ClientResponseDto?> GetByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (client == null)
            return null;

        return MapToDto(client);
    }

    public async Task<IEnumerable<ClientResponseDto>> GetAllAsync(Guid chamberId, int page = 1, int pageSize = 10)
    {
        var clients = await _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return clients.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientResponseDto>> SearchAsync(string query, Guid chamberId)
    {
        var term = query.ToLower();
        var clients = await _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                (c.Name.ToLower().Contains(term) ||
                 c.Email.ToLower().Contains(term) ||
                 c.Phone.Contains(term) ||
                 (c.Nid != null && c.Nid.Contains(term)) ||
                 (c.CompanyName != null && c.CompanyName.ToLower().Contains(term))))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return clients.Select(MapToDto);
    }

    public async Task<int> GetCountAsync(Guid chamberId)
    {
        return await _context.Clients
            .CountAsync(c => c.ChamberId == chamberId && !c.IsDeleted);
    }

    private static ClientResponseDto MapToDto(Client c)
    {
        return new ClientResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            Nid = c.Nid,
            CompanyName = c.CompanyName,
            Notes = c.Notes,
            IsActive = c.IsActive,
            CasesCount = c.ClientCases.Count,
            CreatedAt = c.CreatedAt
        };
    }
}
