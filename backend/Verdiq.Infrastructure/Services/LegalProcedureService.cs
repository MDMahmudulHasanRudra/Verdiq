using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.LegalProcedure;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class LegalProcedureService : ILegalProcedureService
{
    private readonly AppDbContext _db;

    public LegalProcedureService(AppDbContext db) => _db = db;

    public async Task<(bool Success, string Message, LegalProcedureResponseDto? Data)> CreateAsync(CreateLegalProcedureDto dto)
    {
        var sectionExists = await _db.LegalSections.AnyAsync(e => e.Id == dto.LegalSectionId && !e.IsDeleted);
        if (!sectionExists) return (false, "Legal section not found", null);

        var entity = new LegalProcedure
        {
            LegalSectionId = dto.LegalSectionId,
            StepNumber = dto.StepNumber,
            Title = dto.Title,
            Description = dto.Description,
            RequiredDocuments = dto.RequiredDocuments,
            RecommendedTimeline = dto.RecommendedTimeline,
            ResponsibleRole = dto.ResponsibleRole,
            IsMandatory = dto.IsMandatory,
        };

        _db.LegalProcedures.Add(entity);
        await _db.SaveChangesAsync();
        return (true, "Legal procedure created", Map(entity));
    }

    public async Task<(bool Success, string Message, LegalProcedureResponseDto? Data)> UpdateAsync(Guid id, UpdateLegalProcedureDto dto)
    {
        var entity = await _db.LegalProcedures.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (entity == null) return (false, "Legal procedure not found", null);

        if (dto.StepNumber.HasValue) entity.StepNumber = dto.StepNumber.Value;
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.RequiredDocuments != null) entity.RequiredDocuments = dto.RequiredDocuments;
        if (dto.RecommendedTimeline != null) entity.RecommendedTimeline = dto.RecommendedTimeline;
        if (dto.ResponsibleRole != null) entity.ResponsibleRole = dto.ResponsibleRole;
        if (dto.IsMandatory.HasValue) entity.IsMandatory = dto.IsMandatory.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Legal procedure updated", Map(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var entity = await _db.LegalProcedures.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (entity == null) return (false, "Legal procedure not found");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Legal procedure deleted");
    }

    public async Task<LegalProcedureResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _db.LegalProcedures.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<LegalProcedureResponseDto>> GetBySectionAsync(Guid legalSectionId)
    {
        var entities = await _db.LegalProcedures
            .Where(e => e.LegalSectionId == legalSectionId && !e.IsDeleted)
            .OrderBy(e => e.StepNumber)
            .ToListAsync();
        return entities.Select(Map);
    }

    public async Task<IEnumerable<CaseLegalProcedureResponseDto>> GetCaseProceduresAsync(Guid caseId)
    {
        var entities = await _db.CaseLegalProcedures
            .Include(cp => cp.LegalProcedure)
            .Where(cp => cp.CaseLegalSection.CaseId == caseId && !cp.IsDeleted)
            .OrderBy(cp => cp.LegalProcedure.StepNumber)
            .ToListAsync();

        return entities.Select(cp => new CaseLegalProcedureResponseDto
        {
            Id = cp.Id,
            CaseLegalSectionId = cp.CaseLegalSectionId,
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
        });
    }

    public async Task<(bool Success, string Message)> CompleteCaseProcedureAsync(Guid caseProcedureId, string completedBy)
    {
        var cp = await _db.CaseLegalProcedures
            .Include(x => x.LegalProcedure)
            .FirstOrDefaultAsync(x => x.Id == caseProcedureId && !x.IsDeleted);
        if (cp == null) return (false, "Case procedure not found");

        cp.IsCompleted = true;
        cp.CompletedAt = DateTime.UtcNow;
        cp.CompletedBy = completedBy;
        cp.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, $"Procedure '{cp.LegalProcedure.Title}' marked as completed");
    }

    public async Task<(bool Success, string Message)> GenerateCaseProceduresAsync(Guid caseId, Guid legalSectionId)
    {
        var caseExists = await _db.Cases.AnyAsync(c => c.Id == caseId && !c.IsDeleted);
        if (!caseExists) return (false, "Case not found");

        var caseLegalSection = await _db.CaseLegalSections
            .FirstOrDefaultAsync(cls => cls.CaseId == caseId && cls.LegalSectionId == legalSectionId);

        if (caseLegalSection == null)
            return (false, "Legal section not linked to this case");

        var procedures = await _db.LegalProcedures
            .Where(p => p.LegalSectionId == legalSectionId && !p.IsDeleted)
            .ToListAsync();

        if (procedures.Count == 0) return (false, "No procedures found for this legal section");

        var existing = await _db.CaseLegalProcedures
            .Where(cp => cp.CaseLegalSectionId == caseLegalSection.Id)
            .Select(cp => cp.LegalProcedureId)
            .ToListAsync();

        var toAdd = procedures.Where(p => !existing.Contains(p.Id)).Select(p => new CaseLegalProcedure
        {
            CaseLegalSectionId = caseLegalSection.Id,
            LegalProcedureId = p.Id,
        });

        _db.CaseLegalProcedures.AddRange(toAdd);
        await _db.SaveChangesAsync();
        return (true, $"{toAdd.Count()} procedure(s) generated for this case");
    }

    private static LegalProcedureResponseDto Map(LegalProcedure e) => new()
    {
        Id = e.Id,
        LegalSectionId = e.LegalSectionId,
        StepNumber = e.StepNumber,
        Title = e.Title,
        Description = e.Description,
        RequiredDocuments = e.RequiredDocuments,
        RecommendedTimeline = e.RecommendedTimeline,
        ResponsibleRole = e.ResponsibleRole,
        IsMandatory = e.IsMandatory,
        CreatedAt = e.CreatedAt,
    };
}
