using Verdiq.Application.DTOs.Search;

namespace Verdiq.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(Guid userId, string query, int limit = 10);
}
