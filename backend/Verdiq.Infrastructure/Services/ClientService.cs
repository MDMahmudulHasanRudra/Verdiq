using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public ClientService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ClientResponseDto> GetClientByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.Cases.Where(cs => !cs.IsDeleted))
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (client == null)
            throw new KeyNotFoundException("Client not found");

        return MapToDto(client);
    }

    public async Task<IEnumerable<ClientResponseDto>> GetAllClientsAsync(Guid? lawyerId = null)
    {
        var query = _context.Clients
            .Include(c => c.Cases.Where(cs => !cs.IsDeleted))
            .Include(c => c.Payments)
            .Where(c => !c.IsDeleted);

        if (lawyerId.HasValue)
            query = query.Where(c => c.AssignedLawyerId == lawyerId.Value);

        var clients = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return clients.Select(MapToDto);
    }

    public async Task<ClientResponseDto> CreateClientAsync(CreateClientDto dto, Guid lawyerId)
    {
        var client = new Client
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            NationalId = dto.NationalId,
            Notes = dto.Notes,
            IsActive = true,
            AssignedLawyerId = lawyerId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.CompleteAsync();

        return await GetClientByIdAsync(client.Id);
    }

    public async Task<ClientResponseDto> UpdateClientAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            throw new KeyNotFoundException("Client not found");

        if (dto.FullName != null) client.FullName = dto.FullName;
        if (dto.Email != null) client.Email = dto.Email;
        if (dto.Phone != null) client.Phone = dto.Phone;
        if (dto.Address != null) client.Address = dto.Address;
        if (dto.NationalId != null) client.NationalId = dto.NationalId;
        if (dto.Notes != null) client.Notes = dto.Notes;
        if (dto.IsActive.HasValue) client.IsActive = dto.IsActive.Value;

        await _unitOfWork.Clients.UpdateAsync(client);
        await _unitOfWork.CompleteAsync();

        return await GetClientByIdAsync(id);
    }

    public async Task DeleteClientAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            throw new KeyNotFoundException("Client not found");

        await _unitOfWork.Clients.DeleteAsync(client);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<ClientResponseDto>> SearchClientsAsync(string searchTerm, Guid? lawyerId = null)
    {
        var term = searchTerm.ToLower();
        var query = _context.Clients
            .Include(c => c.Cases.Where(cs => !cs.IsDeleted))
            .Include(c => c.Payments)
            .Where(c => !c.IsDeleted &&
                (c.FullName.ToLower().Contains(term) ||
                 c.Email.ToLower().Contains(term) ||
                 c.Phone.Contains(term) ||
                 c.NationalId!.Contains(term)));

        if (lawyerId.HasValue)
            query = query.Where(c => c.AssignedLawyerId == lawyerId.Value);

        var clients = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return clients.Select(MapToDto);
    }

    private static ClientResponseDto MapToDto(Client c)
    {
        return new ClientResponseDto
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            NationalId = c.NationalId,
            Notes = c.Notes,
            IsActive = c.IsActive,
            CasesCount = c.Cases.Count,
            JoinedDate = c.CreatedAt,
            TotalPayments = c.Payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Sum(p => p.Amount)
        };
    }
}
