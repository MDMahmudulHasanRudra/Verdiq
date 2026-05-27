using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class CaseService : ICaseService
{
    private readonly AppDbContext _context;

    public CaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, CaseResponseDto? Data)> CreateAsync(CreateCaseDto dto, Guid userId, Guid chamberId)
    {
        var caseNumber = await GenerateCaseNumberAsync();

        var caseEntity = new Case
        {
            CaseNumber = caseNumber,
            Title = dto.Title,
            CaseType = dto.CaseType,
            Status = CaseStatus.Pending,
            Priority = Enum.TryParse<CasePriority>(dto.Priority, true, out var priority) ? priority : CasePriority.Medium,
            CourtName = dto.CourtName,
            Opponent = dto.Opponent,
            FirNumber = dto.FirNumber,
            PoliceStation = dto.PoliceStation,
            ActsAndSections = dto.ActsAndSections,
            Description = dto.Description,
            FilingDate = dto.FilingDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(dto.FilingDate, DateTimeKind.Utc),
            AssignedLawyerId = userId,
            ChamberId = chamberId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cases.Add(caseEntity);

        foreach (var clientId in dto.ClientIds)
        {
            _context.ClientCases.Add(new ClientCase
            {
                ClientId = clientId,
                CaseId = caseEntity.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseEntity.Id,
            ActivityType = ActivityType.Note,
            Description = $"Case created: {dto.Title}",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(caseEntity.Id);
        return (true, "Case created successfully", result);
    }

    public async Task<(bool Success, string Message, CaseResponseDto? Data)> UpdateAsync(Guid id, UpdateCaseDto dto)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.ClientCases)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (caseEntity == null)
            return (false, "Case not found", null);

        if (dto.Title != null) caseEntity.Title = dto.Title;
        if (dto.CourtName != null) caseEntity.CourtName = dto.CourtName;
        if (dto.CaseType != null) caseEntity.CaseType = dto.CaseType;
        if (dto.Status != null && Enum.TryParse<CaseStatus>(dto.Status, true, out var status))
            caseEntity.Status = status;
        if (dto.Priority != null && Enum.TryParse<CasePriority>(dto.Priority, true, out var priority))
            caseEntity.Priority = priority;
        if (dto.Opponent != null) caseEntity.Opponent = dto.Opponent;
        if (dto.Description != null) caseEntity.Description = dto.Description;
        if (dto.ActsAndSections != null) caseEntity.ActsAndSections = dto.ActsAndSections;

        if (caseEntity.Status == CaseStatus.Closed)
            caseEntity.ClosingDate = DateTime.UtcNow;

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseEntity.Id,
            ActivityType = ActivityType.StatusChange,
            Description = $"Case updated: {(dto.Title ?? caseEntity.Title)}",
            CreatedBy = caseEntity.AssignedLawyerId,
            CreatedAt = DateTime.UtcNow
        });

        if (dto.ClientIds != null)
        {
            var existingIds = caseEntity.ClientCases.Select(cc => cc.ClientId).ToHashSet();
            var newIds = dto.ClientIds.ToHashSet();

            foreach (var cc in caseEntity.ClientCases.Where(cc => !newIds.Contains(cc.ClientId)).ToList())
                _context.ClientCases.Remove(cc);

            foreach (var clientId in newIds.Where(id => !existingIds.Contains(id)))
            {
                _context.ClientCases.Add(new ClientCase
                {
                    ClientId = clientId,
                    CaseId = caseEntity.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        caseEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Case updated successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity == null || caseEntity.IsDeleted)
            return (false, "Case not found");

        caseEntity.IsDeleted = true;
        caseEntity.UpdatedAt = DateTime.UtcNow;

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseEntity.Id,
            ActivityType = ActivityType.Note,
            Description = $"Case deleted: {caseEntity.Title}",
            CreatedBy = caseEntity.AssignedLawyerId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return (true, "Case deleted successfully");
    }

    public async Task<CaseResponseDto?> GetByIdAsync(Guid id)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (caseEntity == null)
            return null;

        return MapToDto(caseEntity);
    }

    public async Task<IEnumerable<CaseResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? priority = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CaseStatus>(status, true, out var caseStatus))
            query = query.Where(c => c.Status == caseStatus);

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<CasePriority>(priority, true, out var casePriority))
            query = query.Where(c => c.Priority == casePriority);

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.CaseNumber.ToLower().Contains(term) ||
                c.Title.ToLower().Contains(term) ||
                c.CourtName.ToLower().Contains(term) ||
                (c.Opponent != null && c.Opponent.ToLower().Contains(term)) ||
                c.ClientCases.Any(cc => cc.Client.Name.ToLower().Contains(term)));
        }

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("casenumber", "asc") => query.OrderBy(c => c.CaseNumber),
            ("casenumber", "desc") => query.OrderByDescending(c => c.CaseNumber),
            ("title", "asc") => query.OrderBy(c => c.Title),
            ("title", "desc") => query.OrderByDescending(c => c.Title),
            ("status", "asc") => query.OrderBy(c => c.Status),
            ("status", "desc") => query.OrderByDescending(c => c.Status),
            ("priority", "asc") => query.OrderBy(c => c.Priority),
            ("priority", "desc") => query.OrderByDescending(c => c.Priority),
            ("filingdate", "asc") => query.OrderBy(c => c.FilingDate),
            ("filingdate", "desc") => query.OrderByDescending(c => c.FilingDate),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var cases = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return cases.Select(MapToDto);
    }

    public async Task<IEnumerable<CaseResponseDto>> SearchAsync(string query, Guid chamberId)
    {
        var term = query.ToLower();
        var cases = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                (c.CaseNumber.ToLower().Contains(term) ||
                 c.Title.ToLower().Contains(term) ||
                 c.CourtName.ToLower().Contains(term) ||
                 (c.Opponent != null && c.Opponent.ToLower().Contains(term)) ||
                 (c.FirNumber != null && c.FirNumber.ToLower().Contains(term)) ||
                 c.ClientCases.Any(cc => cc.Client.Name.ToLower().Contains(term))))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return cases.Select(MapToDto);
    }

    public async Task<int> GetCountAsync(Guid chamberId)
    {
        return await _context.Cases
            .CountAsync(c => c.ChamberId == chamberId && !c.IsDeleted);
    }

    private async Task<string> GenerateCaseNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Cases
            .CountAsync(c => c.CreatedAt.Year == year) + 1;
        return $"VER-{year}-{count:D4}";
    }

    private static CaseResponseDto MapToDto(Case c)
    {
        return new CaseResponseDto
        {
            Id = c.Id,
            CaseNumber = c.CaseNumber,
            Title = c.Title,
            CourtName = c.CourtName,
            CaseType = c.CaseType,
            FilingDate = c.FilingDate,
            Opponent = c.Opponent,
            Status = c.Status.ToString(),
            Priority = c.Priority.ToString(),
            Description = c.Description,
            ActsAndSections = c.ActsAndSections,
            ClosingDate = c.ClosingDate,
            AssignedLawyerId = c.AssignedLawyerId,
            AssignedLawyerName = c.AssignedLawyer.FullName,
            Clients = c.ClientCases
                .Where(cc => !cc.IsDeleted)
                .Select(cc => new ClientInfo
                {
                    Id = cc.Client.Id,
                    Name = cc.Client.Name,
                    Phone = cc.Client.Phone
                }).ToList(),
            HearingsCount = c.Hearings.Count(h => !h.IsDeleted),
            DocumentsCount = c.Documents.Count(d => !d.IsDeleted),
            CreatedAt = c.CreatedAt
        };
    }
}
