using Verdiq.Application.DTOs.Client;

namespace Verdiq.Application.Interfaces;

public interface IClientPastAffairService
{
    Task<(bool Success, string Message, ClientPastAffairResponseDto? Data)> CreateAsync(Guid clientId, CreateClientPastAffairDto dto, Guid chamberId);
    Task<(bool Success, string Message, ClientPastAffairResponseDto? Data)> UpdateAsync(Guid id, UpdateClientPastAffairDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<ClientPastAffairResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClientPastAffairResponseDto>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<ClientPastAffairResponseDto>> GetAllAsync(Guid chamberId, bool? isCriminal = null);
}
