using Verdiq.Application.DTOs.Accounting;

namespace Verdiq.Application.Interfaces;

public interface IChartOfAccountService
{
    Task<AccountResponseDto> CreateAsync(CreateAccountDto dto, Guid chamberId);
    Task<AccountResponseDto> UpdateAsync(Guid id, UpdateAccountDto dto);
    Task DeleteAsync(Guid id);
    Task<AccountResponseDto?> GetByIdAsync(Guid id);
    Task<List<AccountResponseDto>> GetAllAsync(Guid chamberId);
    Task<List<AccountResponseDto>> GetTreeAsync(Guid chamberId);
}
