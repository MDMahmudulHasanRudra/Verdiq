using Verdiq.Application.DTOs.Client;
using Verdiq.Application.DTOs.ClientPortal;

namespace Verdiq.Application.Interfaces;

public interface IClientService
{
    Task<(bool Success, string Message, ClientResponseDto? Data)> CreateAsync(CreateClientDto dto, Guid chamberId);
    Task<(bool Success, string Message, ClientResponseDto? Data)> UpdateAsync(Guid id, UpdateClientDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<ClientResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClientResponseDto>> GetAllAsync(Guid chamberId, int page = 1, int pageSize = 10, string? search = null, string? status = null, string? clientType = null);
    Task<IEnumerable<ClientResponseDto>> SearchAsync(string query, Guid chamberId);
    Task<int> GetCountAsync(Guid chamberId, string? search = null, string? status = null, string? clientType = null);
    Task<(bool Success, string Message, ClientResponseDto? Data)> UploadAvatarAsync(Guid clientId, string avatarUrl);
    Task<IEnumerable<ClientCaseSummaryDto>> GetCasesAsync(Guid clientId, Guid chamberId);
    Task<IEnumerable<ClientHearingDto>> GetHearingsAsync(Guid clientId, Guid chamberId);
}
