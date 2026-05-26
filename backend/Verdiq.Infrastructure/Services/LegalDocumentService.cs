using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.LegalDocument;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class LegalDocumentService : ILegalDocumentService
{
    private readonly AppDbContext _context;

    public LegalDocumentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, LegalDocumentResponseDto? Data)> CreateAsync(CreateLegalDocumentDto dto)
    {
        var document = new LegalDocument
        {
            Title = dto.Title,
            Category = Enum.TryParse<LegalDocumentCategory>(dto.Category, true, out var category) ? category : LegalDocumentCategory.Other,
            Content = dto.Content,
            Citation = dto.Citation,
            JudgeName = dto.JudgeName,
            Keywords = dto.Keywords,
            Year = dto.Year,
            CreatedAt = DateTime.UtcNow
        };

        _context.LegalDocuments.Add(document);
        await _context.SaveChangesAsync();

        var result = MapToDto(document);
        return (true, "Legal document created successfully", result);
    }

    public async Task<IEnumerable<LegalDocumentResponseDto>> SearchAsync(string query)
    {
        var term = query.ToLower();
        var documents = await _context.LegalDocuments
            .Where(d =>
                d.Title.ToLower().Contains(term) ||
                (d.Keywords != null && d.Keywords.ToLower().Contains(term)) ||
                (d.Citation != null && d.Citation.ToLower().Contains(term)) ||
                (d.JudgeName != null && d.JudgeName.ToLower().Contains(term)))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<LegalDocumentResponseDto>> GetByCategoryAsync(string category)
    {
        if (!Enum.TryParse<LegalDocumentCategory>(category, true, out var parsedCategory))
            return Enumerable.Empty<LegalDocumentResponseDto>();

        var documents = await _context.LegalDocuments
            .Where(d => d.Category == parsedCategory)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<LegalDocumentResponseDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var documents = await _context.LegalDocuments
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    private static LegalDocumentResponseDto MapToDto(LegalDocument d)
    {
        return new LegalDocumentResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category.ToString(),
            Content = d.Content,
            Citation = d.Citation,
            JudgeName = d.JudgeName,
            Keywords = d.Keywords,
            Year = d.Year,
            CreatedAt = d.CreatedAt
        };
    }
}
