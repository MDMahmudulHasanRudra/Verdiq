using Verdiq.Application.DTOs.Lead;

namespace Verdiq.Application.Interfaces;

public interface ILeadService
{
    Task<IEnumerable<LeadResponseDto>> GetAllAsync(Guid chamberId);
    Task<LeadResponseDto?> GetByIdAsync(Guid id, Guid chamberId);
    Task<LeadResponseDto> CreateAsync(CreateLeadDto dto, Guid chamberId, Guid userId);
    Task<LeadResponseDto?> UpdateAsync(Guid id, UpdateLeadDto dto, Guid chamberId);
    Task<LeadResponseDto?> UpdateStageAsync(Guid id, UpdateLeadStageDto dto, Guid chamberId);
    Task<bool> DeleteAsync(Guid id, Guid chamberId);
    Task<LeadAnalyticsDto> GetAnalyticsAsync(Guid chamberId);
    Task<IEnumerable<LeadResponseDto>> GetByStageAsync(string stage, Guid chamberId);
}
