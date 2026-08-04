using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ClientPastAffairService : IClientPastAffairService
{
    private readonly AppDbContext _context;

    public ClientPastAffairService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string Message, ClientPastAffairResponseDto? Data)> CreateAsync(Guid clientId, CreateClientPastAffairDto dto, Guid chamberId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null || client.IsDeleted || client.ChamberId != chamberId)
            return (false, "Client not found", null);

        var affair = new ClientPastAffair
        {
            ClientId = clientId,
            ChamberId = chamberId,
            CaseTitle = dto.CaseTitle,
            CaseNumber = dto.CaseNumber,
            CourtName = dto.CourtName,
            CaseType = dto.CaseType,
            Status = dto.Status,
            FilingDate = dto.FilingDate.HasValue ? DateTime.SpecifyKind(dto.FilingDate.Value, DateTimeKind.Utc) : null,
            ClosingDate = dto.ClosingDate.HasValue ? DateTime.SpecifyKind(dto.ClosingDate.Value, DateTimeKind.Utc) : null,
            Opponent = dto.Opponent,
            JudgeName = dto.JudgeName,
            Verdict = dto.Verdict,
            Description = dto.Description,
            ActsAndSections = dto.ActsAndSections,
            LawyerName = dto.LawyerName,
            IsCriminal = dto.IsCriminal,
            Outcome = dto.Outcome,
            Notes = dto.Notes,
        };

        _context.ClientPastAffairs.Add(affair);

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = clientId,
            ActivityType = ActivityType.Note,
            Description = $"Past affair added: {dto.CaseTitle} ({(dto.IsCriminal ? "Criminal" : "Civil")})",
        });

        await _context.SaveChangesAsync();
        var result = await GetByIdAsync(affair.Id);
        return (true, "Past affair recorded", result);
    }

    public async Task<(bool Success, string Message, ClientPastAffairResponseDto? Data)> UpdateAsync(Guid id, UpdateClientPastAffairDto dto)
    {
        var affair = await _context.ClientPastAffairs.FindAsync(id);
        if (affair == null || affair.IsDeleted)
            return (false, "Record not found", null);

        if (dto.CaseTitle != null) affair.CaseTitle = dto.CaseTitle;
        if (dto.CaseNumber != null) affair.CaseNumber = dto.CaseNumber;
        if (dto.CourtName != null) affair.CourtName = dto.CourtName;
        if (dto.CaseType != null) affair.CaseType = dto.CaseType;
        if (dto.Status != null) affair.Status = dto.Status;
        if (dto.FilingDate.HasValue) affair.FilingDate = DateTime.SpecifyKind(dto.FilingDate.Value, DateTimeKind.Utc);
        if (dto.ClosingDate.HasValue) affair.ClosingDate = DateTime.SpecifyKind(dto.ClosingDate.Value, DateTimeKind.Utc);
        if (dto.Opponent != null) affair.Opponent = dto.Opponent;
        if (dto.JudgeName != null) affair.JudgeName = dto.JudgeName;
        if (dto.Verdict != null) affair.Verdict = dto.Verdict;
        if (dto.Description != null) affair.Description = dto.Description;
        if (dto.ActsAndSections != null) affair.ActsAndSections = dto.ActsAndSections;
        if (dto.LawyerName != null) affair.LawyerName = dto.LawyerName;
        if (dto.IsCriminal.HasValue) affair.IsCriminal = dto.IsCriminal.Value;
        if (dto.Outcome != null) affair.Outcome = dto.Outcome;
        if (dto.Notes != null) affair.Notes = dto.Notes;

        affair.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        var result = await GetByIdAsync(id);
        return (true, "Record updated", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var affair = await _context.ClientPastAffairs.FindAsync(id);
        if (affair == null || affair.IsDeleted)
            return (false, "Record not found");

        affair.IsDeleted = true;
        affair.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "Record deleted");
    }

    public async Task<ClientPastAffairResponseDto?> GetByIdAsync(Guid id)
    {
        var affair = await _context.ClientPastAffairs
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return affair == null ? null : MapToDto(affair);
    }

    public async Task<IEnumerable<ClientPastAffairResponseDto>> GetByClientIdAsync(Guid clientId)
    {
        var affairs = await _context.ClientPastAffairs
            .Include(a => a.Client)
            .Where(a => a.ClientId == clientId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return affairs.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientPastAffairResponseDto>> GetAllAsync(Guid chamberId, bool? isCriminal = null)
    {
        var query = _context.ClientPastAffairs
            .Include(a => a.Client)
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted);

        if (isCriminal.HasValue)
            query = query.Where(a => a.IsCriminal == isCriminal.Value);

        var affairs = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return affairs.Select(MapToDto);
    }

    private static ClientPastAffairResponseDto MapToDto(ClientPastAffair a) => new()
    {
        Id = a.Id,
        ClientId = a.ClientId,
        ClientName = a.Client?.Name ?? "Unknown",
        CaseTitle = a.CaseTitle,
        CaseNumber = a.CaseNumber,
        CourtName = a.CourtName,
        CaseType = a.CaseType,
        Status = a.Status,
        FilingDate = a.FilingDate,
        ClosingDate = a.ClosingDate,
        Opponent = a.Opponent,
        JudgeName = a.JudgeName,
        Verdict = a.Verdict,
        Description = a.Description,
        ActsAndSections = a.ActsAndSections,
        LawyerName = a.LawyerName,
        IsCriminal = a.IsCriminal,
        Outcome = a.Outcome,
        Notes = a.Notes,
        DocumentCount = 0,
        CreatedAt = a.CreatedAt,
    };
}
