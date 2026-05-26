using Verdiq.Application.DTOs.Chamber;

namespace Verdiq.Application.Interfaces;

public interface IChamberService
{
    Task<(bool Success, string Message, ChamberResponseDto? Data)> CreateAsync(CreateChamberDto dto, Guid ownerId);
    Task<(bool Success, string Message, ChamberResponseDto? Data)> UpdateAsync(Guid id, UpdateChamberDto dto);
    Task<ChamberResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChamberResponseDto>> GetAllAsync();
}
