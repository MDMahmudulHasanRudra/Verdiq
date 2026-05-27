using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Interfaces;

public interface ICaseService
{
    Task<(bool Success, string Message, CaseResponseDto? Data)> CreateAsync(CreateCaseDto dto, Guid userId, Guid chamberId);
    Task<(bool Success, string Message, CaseResponseDto? Data)> UpdateAsync(Guid id, UpdateCaseDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<CaseResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CaseResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? priority = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 10);
    Task<IEnumerable<CaseResponseDto>> SearchAsync(string query, Guid chamberId);
    Task<int> GetCountAsync(Guid chamberId);
}
