using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class CaseService : ICaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public CaseService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<CaseResponseDto> GetCaseByIdAsync(Guid id)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Client)
            .Include(c => c.AssignedLawyer)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (caseEntity == null)
            throw new KeyNotFoundException("Case not found");

        return MapToDto(caseEntity);
    }

    public async Task<IEnumerable<CaseResponseDto>> GetAllCasesAsync(Guid? lawyerId = null)
    {
        var query = _context.Cases
            .Include(c => c.Client)
            .Include(c => c.AssignedLawyer)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Where(c => !c.IsDeleted);

        if (lawyerId.HasValue)
            query = query.Where(c => c.AssignedLawyerId == lawyerId.Value);

        var cases = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return cases.Select(MapToDto);
    }

    public async Task<CaseResponseDto> CreateCaseAsync(CreateCaseDto dto, Guid lawyerId)
    {
        var caseNumber = await GenerateCaseNumberAsync();

        var caseEntity = new Case
        {
            CaseNumber = caseNumber,
            Title = dto.Title,
            CaseType = dto.CaseType,
            Status = CaseStatus.Pending,
            Priority = Enum.TryParse<CasePriority>(dto.Priority, true, out var priority) ? priority : CasePriority.Medium,
            Court = dto.Court,
            CourtRoom = dto.CourtRoom,
            JudgeName = dto.JudgeName,
            FirNumber = dto.FirNumber,
            PoliceStation = dto.PoliceStation,
            ActsAndSections = dto.ActsAndSections,
            Description = dto.Description,
            FilingDate = DateTime.UtcNow,
            ClientId = dto.ClientId,
            AssignedLawyerId = lawyerId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Cases.AddAsync(caseEntity);
        await _unitOfWork.CompleteAsync();

        return await GetCaseByIdAsync(caseEntity.Id);
    }

    public async Task<CaseResponseDto> UpdateCaseAsync(Guid id, UpdateCaseDto dto)
    {
        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity == null || caseEntity.IsDeleted)
            throw new KeyNotFoundException("Case not found");

        if (dto.Title != null) caseEntity.Title = dto.Title;
        if (dto.CaseType != null) caseEntity.CaseType = dto.CaseType;
        if (dto.Status != null && Enum.TryParse<CaseStatus>(dto.Status, true, out var status))
            caseEntity.Status = status;
        if (dto.Priority != null && Enum.TryParse<CasePriority>(dto.Priority, true, out var priority))
            caseEntity.Priority = priority;
        if (dto.Court != null) caseEntity.Court = dto.Court;
        if (dto.CourtRoom != null) caseEntity.CourtRoom = dto.CourtRoom;
        if (dto.JudgeName != null) caseEntity.JudgeName = dto.JudgeName;
        if (dto.FirNumber != null) caseEntity.FirNumber = dto.FirNumber;
        if (dto.PoliceStation != null) caseEntity.PoliceStation = dto.PoliceStation;
        if (dto.ActsAndSections != null) caseEntity.ActsAndSections = dto.ActsAndSections;
        if (dto.Description != null) caseEntity.Description = dto.Description;

        if (caseEntity.Status == CaseStatus.Closed)
            caseEntity.ClosingDate = DateTime.UtcNow;

        await _unitOfWork.Cases.UpdateAsync(caseEntity);
        await _unitOfWork.CompleteAsync();

        return await GetCaseByIdAsync(id);
    }

    public async Task DeleteCaseAsync(Guid id)
    {
        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity == null || caseEntity.IsDeleted)
            throw new KeyNotFoundException("Case not found");

        await _unitOfWork.Cases.DeleteAsync(caseEntity);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<CaseResponseDto>> SearchCasesAsync(string searchTerm, Guid? lawyerId = null)
    {
        var term = searchTerm.ToLower();
        var query = _context.Cases
            .Include(c => c.Client)
            .Include(c => c.AssignedLawyer)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Where(c => !c.IsDeleted &&
                (c.CaseNumber.ToLower().Contains(term) ||
                 c.Title.ToLower().Contains(term) ||
                 c.Client.FullName.ToLower().Contains(term) ||
                 c.Court.ToLower().Contains(term) ||
                 c.FirNumber!.ToLower().Contains(term)));

        if (lawyerId.HasValue)
            query = query.Where(c => c.AssignedLawyerId == lawyerId.Value);

        var cases = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return cases.Select(MapToDto);
    }

    public async Task<string> GenerateCaseNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Cases
            .CountAsync(c => c.CreatedAt.Year == year) + 1;
        return $"VRD-{year}-{count:D4}";
    }

    private static CaseResponseDto MapToDto(Case c)
    {
        var nextHearing = c.Hearings
            .Where(h => h.HearingDate >= DateTime.UtcNow && h.Status == HearingStatus.Scheduled)
            .OrderBy(h => h.HearingDate)
            .FirstOrDefault();

        return new CaseResponseDto
        {
            Id = c.Id,
            CaseNumber = c.CaseNumber,
            Title = c.Title,
            CaseType = c.CaseType,
            Status = c.Status.ToString(),
            Priority = c.Priority.ToString(),
            Court = c.Court,
            CourtRoom = c.CourtRoom,
            JudgeName = c.JudgeName,
            FirNumber = c.FirNumber,
            PoliceStation = c.PoliceStation,
            ActsAndSections = c.ActsAndSections,
            Description = c.Description,
            FilingDate = c.FilingDate,
            ClosingDate = c.ClosingDate,
            NextHearingDate = nextHearing?.HearingDate,
            ClientId = c.ClientId,
            ClientName = c.Client.FullName,
            AssignedLawyerId = c.AssignedLawyerId,
            AssignedLawyerName = c.AssignedLawyer.FullName,
            DocumentsCount = c.Documents.Count(d => !d.IsDeleted),
            HearingsCount = c.Hearings.Count(h => !h.IsDeleted),
            CreatedAt = c.CreatedAt
        };
    }
}
