using Verdiq.Application.DTOs.Client;

namespace Verdiq.Application.Interfaces;

public interface IClientService
{
    Task<(bool Success, string Message, ClientResponseDto? Data)> CreateAsync(CreateClientDto dto, Guid chamberId);
    Task<(bool Success, string Message, ClientResponseDto? Data)> UpdateAsync(Guid id, UpdateClientDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<ClientResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClientResponseDto>> GetAllAsync(Guid chamberId, int page = 1, int pageSize = 10);
    Task<IEnumerable<ClientResponseDto>> SearchAsync(string query, Guid chamberId);
    Task<int> GetCountAsync(Guid chamberId);
    Task<(bool Success, string Message, ClientResponseDto? Data)> UploadAvatarAsync(Guid clientId, string avatarUrl);
}
