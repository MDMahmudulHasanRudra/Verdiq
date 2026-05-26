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

    public async Task<SearchResponse> SearchAsync(string query, Guid chamberId, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new SearchResponse();

        var term = query.ToLowerInvariant();
        var results = new List<SearchResult>();

        var cases = await _context.Cases
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                (c.Title.ToLower().Contains(term) ||
                 c.CaseNumber.ToLower().Contains(term) ||
                 (c.Opponent != null && c.Opponent.ToLower().Contains(term))))
            .Take(limit)
            .Select(c => new SearchResult
            {
                Id = c.Id.ToString(),
                Type = "case",
                Title = c.CaseNumber,
                Subtitle = c.Title,
                Url = $"/lawyer/cases/{c.Id}",
                Status = c.Status.ToString()
            })
            .ToListAsync();

        results.AddRange(cases);

        if (results.Count < limit)
        {
            var remaining = limit - results.Count;
            var clients = await _context.Clients
                .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                    (c.Name.ToLower().Contains(term) ||
                     c.Phone.Contains(term) ||
                     (c.CompanyName != null && c.CompanyName.ToLower().Contains(term))))
                .Take(remaining)
                .Select(c => new SearchResult
                {
                    Id = c.Id.ToString(),
                    Type = "client",
                    Title = c.Name,
                    Subtitle = $"{c.Email} | {c.Phone}",
                    Url = $"/lawyer/clients/{c.Id}",
                    Status = c.IsActive ? "Active" : "Inactive"
                })
                .ToListAsync();

            results.AddRange(clients);
        }

        if (results.Count < limit)
        {
            var remaining = limit - results.Count;
            var hearings = await _context.Hearings
                .Where(h => h.Case.ChamberId == chamberId && !h.IsDeleted &&
                    h.Case.Title.ToLower().Contains(term))
                .Take(remaining)
                .Select(h => new SearchResult
                {
                    Id = h.Id.ToString(),
                    Type = "hearing",
                    Title = $"Hearing - {h.Case.CaseNumber}",
                    Subtitle = $"{h.Case.CourtName} | {h.HearingDate:yyyy-MM-dd}",
                    Url = $"/lawyer/hearings",
                    Status = h.Status.ToString()
                })
                .ToListAsync();

            results.AddRange(hearings);
        }

        return new SearchResponse
        {
            Results = results,
            TotalCount = results.Count
        };
    }
}
