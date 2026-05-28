using Verdiq.Application.DTOs.Workflow;

namespace Verdiq.Application.Interfaces;

public interface IWorkflowTemplateService
{
    Task<(bool Success, string Message, WorkflowTemplateDto? Data)> CreateAsync(CreateWorkflowTemplateDto dto, Guid chamberId);
    Task<(bool Success, string Message, WorkflowTemplateDto? Data)> UpdateAsync(Guid id, UpdateWorkflowTemplateDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<WorkflowTemplateDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkflowTemplateDto>> GetAllAsync(Guid chamberId);
    Task<WorkflowTemplateDto?> GetDefaultAsync(Guid chamberId);
}
