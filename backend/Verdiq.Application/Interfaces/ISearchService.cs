using Verdiq.Application.DTOs.Search;

namespace Verdiq.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(string query, Guid chamberId, int limit = 10);
}
