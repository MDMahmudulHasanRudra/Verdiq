using Verdiq.Application.DTOs.WorkflowProcess;

namespace Verdiq.Application.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllAsync(Guid chamberId);
    Task<WorkflowDto?> GetByIdAsync(Guid id, Guid chamberId);
    Task<(bool Success, string Message, WorkflowDto? Data)> CreateAsync(CreateWorkflowDto dto, Guid chamberId, Guid userId);
    Task<(bool Success, string Message, WorkflowDto? Data)> UpdateAsync(Guid id, UpdateWorkflowDto dto, Guid chamberId);
    Task<(bool Success, string Message, WorkflowDto? Data)> SetActiveAsync(Guid id, bool isActive, Guid chamberId);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid chamberId);

    Task<IEnumerable<CaseWorkflowDto>> GetByCaseIdAsync(Guid caseId);
    Task<CaseWorkflowDto?> GetCaseWorkflowAsync(Guid caseId, Guid caseWorkflowId);
    Task<(bool Success, string Message, CaseWorkflowDto? Data)> LinkAsync(Guid caseId, LinkWorkflowDto dto, Guid userId);
    Task<(bool Success, string Message)> StartStepAsync(Guid caseId, Guid caseWorkflowId, Guid stepId, Guid userId);
    Task<(bool Success, string Message)> CompleteStepAsync(Guid caseId, Guid caseWorkflowId, Guid stepId, string? notes, Guid userId);
    Task<(bool Success, string Message)> CancelAsync(Guid caseId, Guid caseWorkflowId);
    Task<(bool Success, string Message)> UnlinkAsync(Guid caseId, Guid caseWorkflowId);
}
