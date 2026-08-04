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

    public CaseService(AppDbContext context) => _context = context;

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
            GdNumber = dto.GdNumber,
            JudgeName = dto.JudgeName,
            Bench = dto.Bench,
            Prosecutor = dto.Prosecutor,
            OpposingLawyer = dto.OpposingLawyer,
            Jurisdiction = dto.Jurisdiction,
            AppealStatus = dto.AppealStatus,
            RiskLevel = dto.RiskLevel,
            ComplexityScore = dto.ComplexityScore,
            PracticeArea = dto.PracticeArea,
            Department = dto.Department,
            InternalNotes = dto.InternalNotes,
            RetainerAmount = dto.RetainerAmount,
            BillingMethod = dto.BillingMethod,
            FixedFee = dto.FixedFee,
            HourlyRate = dto.HourlyRate,
            BudgetLimit = dto.BudgetLimit,
            ExpenseBudget = dto.ExpenseBudget,
            NextHearingDate = dto.NextHearingDate.HasValue ? DateTime.SpecifyKind(dto.NextHearingDate.Value, DateTimeKind.Utc) : null,
            CriticalDeadlines = dto.CriticalDeadlines,
            LimitationExpiry = dto.LimitationExpiry.HasValue ? DateTime.SpecifyKind(dto.LimitationExpiry.Value, DateTimeKind.Utc) : null,
            ActsAndSections = dto.ActsAndSections,
            Description = dto.Description,
            FilingDate = dto.FilingDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(dto.FilingDate, DateTimeKind.Utc),
            AssignedLawyerId = dto.AssignedLawyerId ?? userId,
            TeamId = dto.TeamId,
            ChamberId = chamberId,
        };

        _context.Cases.Add(caseEntity);

        await LinkClients(caseEntity.Id, dto.ClientIds, dto.ClientRoles);
        await LinkLegalSections(caseEntity.Id, dto.LegalSectionIds);

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseEntity.Id,
            ActivityType = ActivityType.Note,
            Description = $"Case created: {dto.Title}",
            CreatedBy = userId,
        });

        await _context.SaveChangesAsync();
        var result = await GetByIdAsync(caseEntity.Id);
        return (true, "Case created successfully", result);
    }

    public async Task<(bool Success, string Message, CaseResponseDto? Data)> UpdateAsync(Guid id, UpdateCaseDto dto)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.ClientCases)
            .Include(c => c.CaseLegalSections)
            .Include(c => c.Team)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (caseEntity == null) return (false, "Case not found", null);

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
        if (dto.FirNumber != null) caseEntity.FirNumber = dto.FirNumber;
        if (dto.PoliceStation != null) caseEntity.PoliceStation = dto.PoliceStation;
        if (dto.GdNumber != null) caseEntity.GdNumber = dto.GdNumber;
        if (dto.JudgeName != null) caseEntity.JudgeName = dto.JudgeName;
        if (dto.Bench != null) caseEntity.Bench = dto.Bench;
        if (dto.Prosecutor != null) caseEntity.Prosecutor = dto.Prosecutor;
        if (dto.OpposingLawyer != null) caseEntity.OpposingLawyer = dto.OpposingLawyer;
        if (dto.Jurisdiction != null) caseEntity.Jurisdiction = dto.Jurisdiction;
        if (dto.AppealStatus != null) caseEntity.AppealStatus = dto.AppealStatus;
        if (dto.RiskLevel != null) caseEntity.RiskLevel = dto.RiskLevel;
        if (dto.ComplexityScore.HasValue) caseEntity.ComplexityScore = dto.ComplexityScore;
        if (dto.PracticeArea != null) caseEntity.PracticeArea = dto.PracticeArea;
        if (dto.Department != null) caseEntity.Department = dto.Department;
        if (dto.InternalNotes != null) caseEntity.InternalNotes = dto.InternalNotes;
        if (dto.RetainerAmount.HasValue) caseEntity.RetainerAmount = dto.RetainerAmount;
        if (dto.BillingMethod != null) caseEntity.BillingMethod = dto.BillingMethod;
        if (dto.FixedFee.HasValue) caseEntity.FixedFee = dto.FixedFee;
        if (dto.HourlyRate.HasValue) caseEntity.HourlyRate = dto.HourlyRate;
        if (dto.BudgetLimit.HasValue) caseEntity.BudgetLimit = dto.BudgetLimit;
        if (dto.ExpenseBudget.HasValue) caseEntity.ExpenseBudget = dto.ExpenseBudget;
        if (dto.NextHearingDate.HasValue) caseEntity.NextHearingDate = DateTime.SpecifyKind(dto.NextHearingDate.Value, DateTimeKind.Utc);
        if (dto.CriticalDeadlines != null) caseEntity.CriticalDeadlines = dto.CriticalDeadlines;
        if (dto.LimitationExpiry.HasValue) caseEntity.LimitationExpiry = DateTime.SpecifyKind(dto.LimitationExpiry.Value, DateTimeKind.Utc);

        if (caseEntity.Status == CaseStatus.Closed)
            caseEntity.ClosingDate = DateTime.UtcNow;

        if (dto.ClientIds != null)
        {
            var existingIds = caseEntity.ClientCases.Select(cc => cc.ClientId).ToHashSet();
            var newIds = dto.ClientIds.ToHashSet();
            foreach (var cc in caseEntity.ClientCases.Where(cc => !newIds.Contains(cc.ClientId)).ToList())
                _context.ClientCases.Remove(cc);
            var roleMap = dto.ClientRoles?.ToDictionary(r => r.ClientId, r => r.Role) ?? new();
            foreach (var clientId in newIds.Where(id => !existingIds.Contains(id)))
            {
                _context.ClientCases.Add(new ClientCase
                {
                    ClientId = clientId, CaseId = caseEntity.Id,
                    Role = roleMap.GetValueOrDefault(clientId),
                });
            }
        }

        if (dto.LegalSectionIds != null)
        {
            var existingSectionIds = caseEntity.CaseLegalSections.Select(cls => cls.LegalSectionId).ToHashSet();
            var newSectionIds = dto.LegalSectionIds.ToHashSet();
            foreach (var cls in caseEntity.CaseLegalSections.Where(cls => !newSectionIds.Contains(cls.LegalSectionId)).ToList())
                _context.CaseLegalSections.Remove(cls);
            foreach (var sectionId in newSectionIds.Where(id => !existingSectionIds.Contains(id)))
            {
                _context.CaseLegalSections.Add(new CaseLegalSection { CaseId = caseEntity.Id, LegalSectionId = sectionId });
            }
        }

        caseEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Case updated successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Email or password is incorrect");

        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity == null || caseEntity.IsDeleted) return (false, "Case not found");

        if (user.ChamberId != caseEntity.ChamberId)
            return (false, "You are not authorized to delete this case");

        caseEntity.IsDeleted = true;
        caseEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "Case deleted successfully");
    }

    public async Task<CaseResponseDto?> GetByIdAsync(Guid id)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.CaseLegalSections).ThenInclude(cls => cls.LegalSection)
            .Include(c => c.CaseLegalSections).ThenInclude(cls => cls.CaseProcedures).ThenInclude(cp => cp.LegalProcedure)
            .Include(c => c.Team)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        return caseEntity == null ? null : MapToDto(caseEntity);
    }

    public async Task<IEnumerable<CaseResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? priority = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 10, string? type = null, string? courtName = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.CaseLegalSections).ThenInclude(cls => cls.LegalSection)
            .Include(c => c.Team)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CaseStatus>(status, true, out var caseStatus))
            query = query.Where(c => c.Status == caseStatus);
        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<CasePriority>(priority, true, out var casePriority))
            query = query.Where(c => c.Priority == casePriority);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.CaseType != null && c.CaseType.ToLower().Contains(type.ToLower()));
        if (!string.IsNullOrEmpty(courtName))
            query = query.Where(c => c.CourtName.ToLower().Contains(courtName.ToLower()));
        if (dateFrom.HasValue)
            query = query.Where(c => c.FilingDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(c => c.FilingDate <= dateTo.Value);
        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.CaseNumber.ToLower().Contains(term) || c.Title.ToLower().Contains(term) ||
                c.CourtName.ToLower().Contains(term) || (c.Opponent != null && c.Opponent.ToLower().Contains(term)) ||
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

        return (await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()).Select(MapToDto);
    }

    public async Task<IEnumerable<CaseResponseDto>> SearchAsync(string query, Guid chamberId)
    {
        var term = query.ToLower();
        var cases = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.CaseLegalSections).ThenInclude(cls => cls.LegalSection)
            .Include(c => c.Team)
            .Include(c => c.Hearings).Include(c => c.Documents)
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                (c.CaseNumber.ToLower().Contains(term) || c.Title.ToLower().Contains(term) ||
                 c.CourtName.ToLower().Contains(term) || (c.Opponent != null && c.Opponent.ToLower().Contains(term)) ||
                 (c.FirNumber != null && c.FirNumber.ToLower().Contains(term)) ||
                 c.ClientCases.Any(cc => cc.Client.Name.ToLower().Contains(term))))
            .OrderByDescending(c => c.CreatedAt).ToListAsync();
        return cases.Select(MapToDto);
    }

    public async Task<(int SuccessCount, int FailCount, string Message)> BulkStatusChangeAsync(BulkStatusChangeDto dto, Guid chamberId)
    {
        var caseIds = dto.CaseIds.ToHashSet();
        var cases = await _context.Cases
            .Where(c => caseIds.Contains(c.Id) && c.ChamberId == chamberId && !c.IsDeleted)
            .ToListAsync();

        if (cases.Count == 0)
            return (0, dto.CaseIds.Count, "No matching cases found");

        if (!Enum.TryParse<CaseStatus>(dto.Status, true, out var status))
            return (0, dto.CaseIds.Count, "Invalid status");

        foreach (var c in cases)
        {
            c.Status = status;
            c.UpdatedAt = DateTime.UtcNow;
            if (status == CaseStatus.Closed)
                c.ClosingDate = DateTime.UtcNow;

            _context.CaseActivities.Add(new CaseActivity
            {
                CaseId = c.Id,
                ActivityType = ActivityType.StatusChange,
                Description = $"Bulk status changed to {status}",
            });
        }

        await _context.SaveChangesAsync();
        return (cases.Count, dto.CaseIds.Count - cases.Count, $"Updated {cases.Count} case(s) to {status}");
    }

    public async Task<(int SuccessCount, int FailCount, string Message)> BulkDeleteAsync(BulkDeleteDto dto, Guid chamberId)
    {
        var caseIds = dto.CaseIds.ToHashSet();
        var cases = await _context.Cases
            .Where(c => caseIds.Contains(c.Id) && c.ChamberId == chamberId && !c.IsDeleted)
            .ToListAsync();

        if (cases.Count == 0)
            return (0, dto.CaseIds.Count, "No matching cases found");

        foreach (var c in cases)
        {
            c.IsDeleted = true;
            c.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (cases.Count, dto.CaseIds.Count - cases.Count, $"Deleted {cases.Count} case(s)");
    }

    public async Task<(bool Success, string Message, CaseResponseDto? Data)> DuplicateAsync(Guid id, Guid userId, Guid chamberId)
    {
        var source = await _context.Cases
            .Include(c => c.ClientCases)
            .Include(c => c.CaseLegalSections)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (source == null) return (false, "Source case not found", null);

        var caseNumber = await GenerateCaseNumberAsync();
        var newCase = new Case
        {
            CaseNumber = caseNumber,
            Title = $"{source.Title} (Copy)",
            CaseType = source.CaseType,
            Status = CaseStatus.Pending,
            Priority = source.Priority,
            CourtName = source.CourtName,
            Opponent = source.Opponent,
            FirNumber = source.FirNumber,
            PoliceStation = source.PoliceStation,
            GdNumber = source.GdNumber,
            JudgeName = source.JudgeName,
            Bench = source.Bench,
            Prosecutor = source.Prosecutor,
            OpposingLawyer = source.OpposingLawyer,
            Jurisdiction = source.Jurisdiction,
            AppealStatus = source.AppealStatus,
            RiskLevel = source.RiskLevel,
            ComplexityScore = source.ComplexityScore,
            PracticeArea = source.PracticeArea,
            Department = source.Department,
            InternalNotes = source.InternalNotes,
            RetainerAmount = source.RetainerAmount,
            BillingMethod = source.BillingMethod,
            FixedFee = source.FixedFee,
            HourlyRate = source.HourlyRate,
            BudgetLimit = source.BudgetLimit,
            ExpenseBudget = source.ExpenseBudget,
            NextHearingDate = source.NextHearingDate,
            CriticalDeadlines = source.CriticalDeadlines,
            LimitationExpiry = source.LimitationExpiry,
            ActsAndSections = source.ActsAndSections,
            Description = source.Description,
            FilingDate = source.FilingDate,
            AssignedLawyerId = userId,
            TeamId = source.TeamId,
            ChamberId = chamberId,
        };

        _context.Cases.Add(newCase);

        foreach (var cc in source.ClientCases.Where(cc => !cc.IsDeleted))
        {
            _context.ClientCases.Add(new ClientCase
            {
                ClientId = cc.ClientId,
                CaseId = newCase.Id,
                Role = cc.Role,
            });
        }

        var sourceSectionIds = source.CaseLegalSections.Where(cls => !cls.IsDeleted).Select(cls => cls.LegalSectionId).ToList();
        foreach (var sectionId in sourceSectionIds)
        {
            _context.CaseLegalSections.Add(new CaseLegalSection
            {
                CaseId = newCase.Id,
                LegalSectionId = sectionId,
            });
        }

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = newCase.Id,
            ActivityType = ActivityType.Note,
            Description = $"Duplicated from {source.CaseNumber}",
            CreatedBy = userId,
        });

        await _context.SaveChangesAsync();
        var result = await GetByIdAsync(newCase.Id);
        return (true, $"Case duplicated as {caseNumber}", result);
    }

    public async Task<int> GetCountAsync(Guid chamberId, string? status = null, string? priority = null, string? type = null, string? courtName = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.Cases.Where(c => c.ChamberId == chamberId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CaseStatus>(status, true, out var caseStatus))
            query = query.Where(c => c.Status == caseStatus);
        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<CasePriority>(priority, true, out var casePriority))
            query = query.Where(c => c.Priority == casePriority);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.CaseType != null && c.CaseType.ToLower().Contains(type.ToLower()));
        if (!string.IsNullOrEmpty(courtName))
            query = query.Where(c => c.CourtName.ToLower().Contains(courtName.ToLower()));
        if (dateFrom.HasValue)
            query = query.Where(c => c.FilingDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(c => c.FilingDate <= dateTo.Value);

        return await query.CountAsync();
    }

    private async System.Threading.Tasks.Task LinkClients(Guid caseId, List<Guid> clientIds, List<ClientRoleDto>? clientRoles)
    {
        var roleMap = clientRoles?.ToDictionary(r => r.ClientId, r => r.Role) ?? new();
        foreach (var clientId in clientIds)
        {
            _context.ClientCases.Add(new ClientCase
            {
                ClientId = clientId,
                CaseId = caseId,
                Role = roleMap.GetValueOrDefault(clientId),
            });
        }
    }

    private async System.Threading.Tasks.Task LinkLegalSections(Guid caseId, List<Guid>? legalSectionIds)
    {
        if (legalSectionIds == null) return;
        var caseLegalSections = legalSectionIds.Select(sectionId => new CaseLegalSection
        {
            CaseId = caseId,
            LegalSectionId = sectionId,
        }).ToList();
        _context.CaseLegalSections.AddRange(caseLegalSections);
        var sectionIds = legalSectionIds.ToHashSet();
        var procedures = await _context.LegalProcedures
            .Where(p => sectionIds.Contains(p.LegalSectionId) && !p.IsDeleted)
            .ToListAsync();
        foreach (var cls in caseLegalSections)
        {
            foreach (var proc in procedures.Where(p => p.LegalSectionId == cls.LegalSectionId))
            {
                _context.CaseLegalProcedures.Add(new CaseLegalProcedure
                {
                    CaseLegalSectionId = cls.Id,
                    LegalProcedureId = proc.Id,
                });
            }
        }
    }

    private async Task<string> GenerateCaseNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Cases.CountAsync(c => c.CreatedAt.Year == year) + 1;
        return $"VER-{year}-{count:D4}";
    }

    private static CaseResponseDto MapToDto(Case c) => new()
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
        TeamId = c.TeamId,
        TeamName = c.Team != null ? c.Team.Name : null,
        FirNumber = c.FirNumber,
        PoliceStation = c.PoliceStation,
        GdNumber = c.GdNumber,
        JudgeName = c.JudgeName,
        Bench = c.Bench,
        Prosecutor = c.Prosecutor,
        OpposingLawyer = c.OpposingLawyer,
        Jurisdiction = c.Jurisdiction,
        AppealStatus = c.AppealStatus,
        RiskLevel = c.RiskLevel,
        ComplexityScore = c.ComplexityScore,
        PracticeArea = c.PracticeArea,
        Department = c.Department,
        InternalNotes = c.InternalNotes,
        RetainerAmount = c.RetainerAmount,
        BillingMethod = c.BillingMethod,
        FixedFee = c.FixedFee,
        HourlyRate = c.HourlyRate,
        BudgetLimit = c.BudgetLimit,
        ExpenseBudget = c.ExpenseBudget,
        NextHearingDate = c.NextHearingDate,
        LastHearingDate = c.Hearings
            .Where(h => !h.IsDeleted)
            .OrderByDescending(h => h.HearingDate)
            .Select(h => (DateTime?)h.HearingDate)
            .FirstOrDefault(),
        LastHearingResult = c.Hearings
            .Where(h => !h.IsDeleted && !string.IsNullOrWhiteSpace(h.Result))
            .OrderByDescending(h => h.HearingDate)
            .Select(h => h.Result)
            .FirstOrDefault(),
        CriticalDeadlines = c.CriticalDeadlines,
        LimitationExpiry = c.LimitationExpiry,
        Clients = c.ClientCases.Where(cc => !cc.IsDeleted).Select(cc => new ClientInfo
        {
            Id = cc.Client.Id,
            Name = cc.Client.Name,
            Phone = cc.Client.Phone,
            Role = cc.Role,
        }).ToList(),
        HearingsCount = c.Hearings.Count(h => !h.IsDeleted),
        DocumentsCount = c.Documents.Count(d => !d.IsDeleted),
        CreatedAt = c.CreatedAt,
        LegalSections = c.CaseLegalSections.Where(cls => !cls.IsDeleted).Select(cls => new LegalSectionInfo
        {
            Id = cls.Id,
            LegalSectionId = cls.LegalSectionId,
            SectionCode = cls.LegalSection.SectionCode,
            SectionTitle = cls.LegalSection.SectionTitle,
            LawName = cls.LegalSection.LawName,
            Procedures = cls.CaseProcedures.Where(cp => !cp.IsDeleted).Select(cp => new CaseProcedureInfo
            {
                Id = cp.Id,
                LegalProcedureId = cp.LegalProcedureId,
                ProcedureTitle = cp.LegalProcedure.Title,
                StepNumber = cp.LegalProcedure.StepNumber,
                Description = cp.LegalProcedure.Description,
                RequiredDocuments = cp.LegalProcedure.RequiredDocuments,
                RecommendedTimeline = cp.LegalProcedure.RecommendedTimeline,
                ResponsibleRole = cp.LegalProcedure.ResponsibleRole,
                IsMandatory = cp.LegalProcedure.IsMandatory,
                IsCompleted = cp.IsCompleted,
                CompletedAt = cp.CompletedAt,
                CompletedBy = cp.CompletedBy,
                Notes = cp.Notes,
            }).ToList(),
        }).ToList(),
    };
}