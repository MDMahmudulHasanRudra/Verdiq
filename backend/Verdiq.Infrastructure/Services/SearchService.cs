using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Search;
using Verdiq.Application.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _context;

    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResponse> SearchAsync(Guid userId, string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new SearchResponse();

        var term = query.ToLowerInvariant();
        var results = new List<SearchResult>();

        var cases = await _context.Cases
            .Where(c => c.AssignedLawyerId == userId && !c.IsDeleted &&
                (c.CaseNumber.ToLower().Contains(term) ||
                 c.Title.ToLower().Contains(term) ||
                 c.CaseType.ToLower().Contains(term) ||
                 c.Court.ToLower().Contains(term)))
            .Take(limit)
            .Select(c => new SearchResult
            {
                Id = c.Id.ToString(),
                Type = "case",
                Title = c.CaseNumber,
                Subtitle = c.Title,
                Url = $"/lawyer/cases/{c.Id}",
                Status = c.Status.ToString(),
            })
            .ToListAsync();

        results.AddRange(cases);

        if (results.Count < limit)
        {
            var remaining = limit - results.Count;
            var clients = await _context.Clients
                .Where(c => c.AssignedLawyerId == userId && !c.IsDeleted &&
                    (c.FullName.ToLower().Contains(term) ||
                     c.Email.ToLower().Contains(term) ||
                     c.Phone.Contains(term)))
                .Take(remaining)
                .Select(c => new SearchResult
                {
                    Id = c.Id.ToString(),
                    Type = "client",
                    Title = c.FullName,
                    Subtitle = $"{c.Email} | {c.Phone}",
                    Url = $"/lawyer/clients/{c.Id}",
                    Status = c.IsActive ? "Active" : "Inactive",
                })
                .ToListAsync();

            results.AddRange(clients);
        }

        if (results.Count < limit)
        {
            var remaining = limit - results.Count;
            var hearings = await _context.Hearings
                .Where(h => h.Case.AssignedLawyerId == userId && !h.IsDeleted &&
                    (h.HearingType.ToLower().Contains(term) ||
                     h.Court.ToLower().Contains(term) ||
                     h.Case.CaseNumber.ToLower().Contains(term)))
                .Take(remaining)
                .Select(h => new SearchResult
                {
                    Id = h.Id.ToString(),
                    Type = "hearing",
                    Title = $"{h.HearingType} - {h.Case.CaseNumber}",
                    Subtitle = $"{h.Court} | {h.HearingDate:yyyy-MM-dd}",
                    Url = $"/lawyer/hearings",
                    Status = h.Status.ToString(),
                })
                .ToListAsync();

            results.AddRange(hearings);
        }

        if (results.Count < limit)
        {
            var remaining = limit - results.Count;
            var documents = await _context.Documents
                .Where(d => d.Case.AssignedLawyerId == userId && !d.IsDeleted &&
                    (d.OriginalFileName.ToLower().Contains(term) ||
                     d.DocumentType.ToLower().Contains(term) ||
                     d.Category.ToLower().Contains(term)))
                .Take(remaining)
                .Select(d => new SearchResult
                {
                    Id = d.Id.ToString(),
                    Type = "document",
                    Title = d.OriginalFileName,
                    Subtitle = $"{d.DocumentType} | {d.Category} | {FormatSize(d.FileSize)}",
                    Url = $"/lawyer/documents",
                    Status = d.Status.ToString(),
                })
                .ToListAsync();

            results.AddRange(documents);
        }

        return new SearchResponse
        {
            Results = results,
            TotalCount = results.Count,
        };
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB",
    };
}
