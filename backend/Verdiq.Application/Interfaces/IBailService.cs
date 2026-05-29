using Verdiq.Application.DTOs.Bail;

namespace Verdiq.Application.Interfaces;

public interface IBailService
{
    Task<(bool Success, string Message, BailResponseDto? Data)> CreateAsync(CreateBailDto dto, Guid chamberId);
    Task<(bool Success, string Message, BailResponseDto? Data)> UpdateAsync(Guid id, UpdateBailDto dto);
    Task<(bool Success, string Message, BailResponseDto? Data)> UpdateStatusAsync(Guid id, UpdateBailStatusDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<BailResponseDto?> GetByIdAsync(Guid id);
    Task<BailResponseDto?> GetByCaseIdAsync(Guid caseId);
    Task<IEnumerable<BailResponseDto>> GetAllAsync(Guid chamberId, string? status = null);
}
