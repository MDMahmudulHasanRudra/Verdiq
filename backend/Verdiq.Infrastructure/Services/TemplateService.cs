using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Template;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly AppDbContext _context;

    public TemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, TemplateResponseDto? Data)> CreateAsync(CreateTemplateDto dto)
    {
        var template = new Template
        {
            Title = dto.Title,
            Category = dto.Category,
            Content = dto.Content,
            Variables = dto.Variables,
            CreatedAt = DateTime.UtcNow
        };

        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var result = MapToDto(template);
        return (true, "Template created successfully", result);
    }

    public async Task<IEnumerable<TemplateResponseDto>> GetAllAsync(string? category = null)
    {
        var query = _context.Templates.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        var templates = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return templates.Select(MapToDto);
    }

    public async Task<TemplateResponseDto?> GetByIdAsync(Guid id)
    {
        var template = await _context.Templates.FindAsync(id);
        return template == null ? null : MapToDto(template);
    }

    public async Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, string> variables)
    {
        var template = await _context.Templates.FindAsync(templateId);
        if (template == null)
            return string.Empty;

        var content = template.Content;
        foreach (var kvp in variables)
        {
            content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return content;
    }

    private static TemplateResponseDto MapToDto(Template t)
    {
        return new TemplateResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Category = t.Category,
            Content = t.Content,
            Variables = t.Variables,
            CreatedAt = t.CreatedAt
        };
    }
}
