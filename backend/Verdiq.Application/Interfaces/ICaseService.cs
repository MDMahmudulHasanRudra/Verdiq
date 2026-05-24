using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Interfaces;

public interface ICaseService
{
    Task<CaseResponseDto> GetCaseByIdAsync(Guid id);
    Task<IEnumerable<CaseResponseDto>> GetAllCasesAsync(Guid? lawyerId = null);
    Task<CaseResponseDto> CreateCaseAsync(CreateCaseDto dto, Guid lawyerId);
    Task<CaseResponseDto> UpdateCaseAsync(Guid id, UpdateCaseDto dto);
    Task DeleteCaseAsync(Guid id);
    Task<IEnumerable<CaseResponseDto>> SearchCasesAsync(string searchTerm, Guid? lawyerId = null);
    Task<string> GenerateCaseNumberAsync();
}
