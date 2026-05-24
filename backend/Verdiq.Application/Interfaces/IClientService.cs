using Verdiq.Application.DTOs.Client;

namespace Verdiq.Application.Interfaces;

public interface IClientService
{
    Task<ClientResponseDto> GetClientByIdAsync(Guid id);
    Task<IEnumerable<ClientResponseDto>> GetAllClientsAsync(Guid? lawyerId = null);
    Task<ClientResponseDto> CreateClientAsync(CreateClientDto dto, Guid lawyerId);
    Task<ClientResponseDto> UpdateClientAsync(Guid id, UpdateClientDto dto);
    Task DeleteClientAsync(Guid id);
    Task<IEnumerable<ClientResponseDto>> SearchClientsAsync(string searchTerm, Guid? lawyerId = null);
}
