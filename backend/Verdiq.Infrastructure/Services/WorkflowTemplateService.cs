using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Workflow;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class WorkflowTemplateService : IWorkflowTemplateService
{
    private readonly AppDbContext _context;

    public WorkflowTemplateService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string Message, WorkflowTemplateDto? Data)> CreateAsync(CreateWorkflowTemplateDto dto, Guid chamberId)
    {
        var template = new WorkflowTemplate
        {
            ChamberId = chamberId,
            Name = dto.Name,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
        };

        if (dto.IsDefault)
            await ClearDefaultFlag(chamberId);

        _context.WorkflowTemplates.Add(template);

        for (int i = 0; i < dto.LegalSectionIds.Count; i++)
        {
            _context.WorkflowTemplateSections.Add(new WorkflowTemplateSection
            {
                TemplateId = template.Id,
                LegalSectionId = dto.LegalSectionIds[i],
                DisplayOrder = i + 1,
            });
        }

        await _context.SaveChangesAsync();
        var result = await GetByIdAsync(template.Id);
        return (true, "Workflow template created", result);
    }

    public async Task<(bool Success, string Message, WorkflowTemplateDto? Data)> UpdateAsync(Guid id, UpdateWorkflowTemplateDto dto)
    {
        var template = await _context.WorkflowTemplates
            .Include(t => t.Sections)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (template == null) return (false, "Template not found", null);

        if (dto.Name != null) template.Name = dto.Name;
        if (dto.Description != null) template.Description = dto.Description;
        if (dto.IsDefault == true)
            await ClearDefaultFlag(template.ChamberId);

        if (dto.IsDefault.HasValue) template.IsDefault = dto.IsDefault.Value;

        if (dto.LegalSectionIds != null)
        {
            _context.WorkflowTemplateSections.RemoveRange(template.Sections);
            for (int i = 0; i < dto.LegalSectionIds.Count; i++)
            {
                _context.WorkflowTemplateSections.Add(new WorkflowTemplateSection
                {
                    TemplateId = template.Id,
                    LegalSectionId = dto.LegalSectionIds[i],
                    DisplayOrder = i + 1,
                });
            }
        }

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Template updated", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var template = await _context.WorkflowTemplates.FindAsync(id);
        if (template == null || template.IsDeleted) return (false, "Template not found");
        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "Template deleted");
    }

    public async Task<WorkflowTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _context.WorkflowTemplates
            .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.LegalSection)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return template == null ? null : MapToDto(template);
    }

    public async Task<IEnumerable<WorkflowTemplateDto>> GetAllAsync(Guid chamberId)
    {
        var templates = await _context.WorkflowTemplates
            .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.LegalSection)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted)
            .OrderByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return templates.Select(MapToDto);
    }

    public async Task<WorkflowTemplateDto?> GetDefaultAsync(Guid chamberId)
    {
        var template = await _context.WorkflowTemplates
            .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.LegalSection)
            .FirstOrDefaultAsync(t => t.ChamberId == chamberId && t.IsDefault && !t.IsDeleted);

        return template == null ? null : MapToDto(template);
    }

    private async System.Threading.Tasks.Task ClearDefaultFlag(Guid chamberId)
    {
        var current = await _context.WorkflowTemplates
            .Where(t => t.ChamberId == chamberId && t.IsDefault)
            .ToListAsync();
        foreach (var t in current) t.IsDefault = false;
    }

    private static WorkflowTemplateDto MapToDto(WorkflowTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        IsDefault = t.IsDefault,
        CreatedAt = t.CreatedAt,
        Sections = t.Sections.Select(s => new WorkflowSectionItem
        {
            Id = s.Id,
            LegalSectionId = s.LegalSectionId,
            SectionCode = s.LegalSection.SectionCode,
            SectionTitle = s.LegalSection.SectionTitle,
            LawName = s.LegalSection.LawName,
            DisplayOrder = s.DisplayOrder,
        }).ToList(),
    };
}
