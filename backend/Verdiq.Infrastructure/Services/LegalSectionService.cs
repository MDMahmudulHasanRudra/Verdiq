using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.LegalSection;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class LegalSectionService : ILegalSectionService
{
    private readonly AppDbContext _db;

    public LegalSectionService(AppDbContext db) => _db = db;

    public async Task<(bool Success, string Message, LegalSectionResponseDto? Data)> CreateAsync(CreateLegalSectionDto dto, Guid chamberId)
    {
        var entity = new LegalSection
        {
            SectionCode = dto.SectionCode,
            SectionTitle = dto.SectionTitle,
            LawName = dto.LawName,
            Country = dto.Country,
            Category = dto.Category,
            Description = dto.Description,
            Severity = dto.Severity,
            ChamberId = chamberId,
        };

        _db.LegalSections.Add(entity);
        await _db.SaveChangesAsync();

        return (true, "Legal section created", Map(entity));
    }

    public async Task<(bool Success, string Message, LegalSectionResponseDto? Data)> UpdateAsync(Guid id, UpdateLegalSectionDto dto)
    {
        var entity = await _db.LegalSections.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (entity == null) return (false, "Legal section not found", null);

        if (dto.SectionCode != null) entity.SectionCode = dto.SectionCode;
        if (dto.SectionTitle != null) entity.SectionTitle = dto.SectionTitle;
        if (dto.LawName != null) entity.LawName = dto.LawName;
        if (dto.Country != null) entity.Country = dto.Country;
        if (dto.Category != null) entity.Category = dto.Category;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Severity != null) entity.Severity = dto.Severity;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Legal section updated", Map(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var entity = await _db.LegalSections.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (entity == null) return (false, "Legal section not found");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Legal section deleted");
    }

    public async Task<LegalSectionResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _db.LegalSections
            .Include(e => e.Procedures)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<LegalSectionResponseDto>> GetAllAsync(Guid chamberId, string? category = null, string? search = null)
    {
        var query = _db.LegalSections.Where(e => e.ChamberId == chamberId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.Category == category);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.SectionCode.Contains(search) || e.SectionTitle.Contains(search) || e.LawName.Contains(search));

        var entities = await query.Include(e => e.Procedures).OrderBy(e => e.SectionCode).ToListAsync();
        return entities.Select(Map);
    }

    public async Task<IEnumerable<LegalSectionResponseDto>> SearchAsync(string query, Guid chamberId)
    {
        var entities = await _db.LegalSections
            .Where(e => e.ChamberId == chamberId && !e.IsDeleted &&
                (e.SectionCode.Contains(query) || e.SectionTitle.Contains(query) || e.LawName.Contains(query)))
            .Include(e => e.Procedures)
            .Take(20)
            .ToListAsync();
        return entities.Select(Map);
    }

    private static LegalSectionResponseDto Map(LegalSection e) => new()
    {
        Id = e.Id,
        SectionCode = e.SectionCode,
        SectionTitle = e.SectionTitle,
        LawName = e.LawName,
        Country = e.Country,
        Category = e.Category,
        Description = e.Description,
        Severity = e.Severity,
        IsActive = e.IsActive,
        ProcedureCount = e.Procedures?.Count ?? 0,
        CreatedAt = e.CreatedAt,
    };
}
