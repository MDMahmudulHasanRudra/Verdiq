using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Chamber;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ChamberService : IChamberService
{
    private readonly AppDbContext _context;

    public ChamberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, ChamberResponseDto? Data)> CreateAsync(CreateChamberDto dto, Guid ownerId)
    {
        var chamber = new Chamber
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            SubscriptionPlan = SubscriptionPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        _context.Chambers.Add(chamber);
        await _context.SaveChangesAsync();

        var owner = await _context.Users.FindAsync(ownerId);
        if (owner != null)
        {
            owner.ChamberId = chamber.Id;
            owner.Role = UserRole.Owner;
            await _context.SaveChangesAsync();
        }

        var result = await GetByIdAsync(chamber.Id);
        return (true, "Chamber created successfully", result);
    }

    public async Task<(bool Success, string Message, ChamberResponseDto? Data)> UpdateAsync(Guid id, UpdateChamberDto dto)
    {
        var chamber = await _context.Chambers.FindAsync(id);
        if (chamber == null)
            return (false, "Chamber not found", null);

        if (dto.Name != null) chamber.Name = dto.Name;
        if (dto.Logo != null) chamber.Logo = dto.Logo;
        if (dto.Address != null) chamber.Address = dto.Address;
        if (dto.Phone != null) chamber.Phone = dto.Phone;

        chamber.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Chamber updated successfully", result);
    }

    public async Task<ChamberResponseDto?> GetByIdAsync(Guid id)
    {
        var chamber = await _context.Chambers
            .Include(c => c.Users)
            .Include(c => c.Cases)
            .Include(c => c.Clients)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chamber == null)
            return null;

        return new ChamberResponseDto
        {
            Id = chamber.Id,
            Name = chamber.Name,
            Logo = chamber.Logo,
            Address = chamber.Address,
            Phone = chamber.Phone,
            SubscriptionPlan = chamber.SubscriptionPlan.ToString(),
            UsersCount = chamber.Users.Count,
            CasesCount = chamber.Cases.Count,
            ClientsCount = chamber.Clients.Count,
            CreatedAt = chamber.CreatedAt
        };
    }

    public async Task<IEnumerable<ChamberResponseDto>> GetAllAsync()
    {
        var chambers = await _context.Chambers
            .Include(c => c.Users)
            .Include(c => c.Cases)
            .Include(c => c.Clients)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return chambers.Select(c => new ChamberResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Logo = c.Logo,
            Address = c.Address,
            Phone = c.Phone,
            SubscriptionPlan = c.SubscriptionPlan.ToString(),
            UsersCount = c.Users.Count,
            CasesCount = c.Cases.Count,
            ClientsCount = c.Clients.Count,
            CreatedAt = c.CreatedAt
        });
    }
}
