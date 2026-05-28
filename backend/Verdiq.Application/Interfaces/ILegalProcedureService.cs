using Verdiq.Application.DTOs.LegalProcedure;

namespace Verdiq.Application.Interfaces;

public interface ILegalProcedureService
{
    Task<(bool Success, string Message, LegalProcedureResponseDto? Data)> CreateAsync(CreateLegalProcedureDto dto);
    Task<(bool Success, string Message, LegalProcedureResponseDto? Data)> UpdateAsync(Guid id, UpdateLegalProcedureDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<LegalProcedureResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LegalProcedureResponseDto>> GetBySectionAsync(Guid legalSectionId);

    Task<IEnumerable<CaseLegalProcedureResponseDto>> GetCaseProceduresAsync(Guid caseId);
    Task<(bool Success, string Message)> CompleteCaseProcedureAsync(Guid caseProcedureId, string completedBy);
    Task<(bool Success, string Message)> GenerateCaseProceduresAsync(Guid caseId, Guid legalSectionId);
}
