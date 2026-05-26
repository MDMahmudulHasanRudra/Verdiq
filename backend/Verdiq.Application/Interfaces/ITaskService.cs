using Verdiq.Application.DTOs.Task;

namespace Verdiq.Application.Interfaces;

public interface ITaskService
{
    Task<(bool Success, string Message, TaskResponseDto? Data)> CreateAsync(CreateTaskDto dto, Guid assignedBy, Guid chamberId);
    Task<(bool Success, string Message, TaskResponseDto? Data)> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId);
    Task<IEnumerable<TaskResponseDto>> GetByCaseIdAsync(Guid caseId);
    Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid chamberId);
}
