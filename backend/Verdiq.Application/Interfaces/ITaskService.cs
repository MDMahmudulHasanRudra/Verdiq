using Verdiq.Application.DTOs.Task;

namespace Verdiq.Application.Interfaces;

public interface ITaskService
{
    Task<(bool Success, string Message, TaskResponseDto? Data)> CreateAsync(CreateTaskDto dto, Guid assignedBy, Guid chamberId);
    Task<(bool Success, string Message, TaskResponseDto? Data)> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId);
    Task<IEnumerable<TaskResponseDto>> GetByCaseIdAsync(Guid caseId);
    Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? priority = null, Guid? assignedTo = null);
    Task<TaskResponseDto?> GetByIdAsync(Guid id);
    Task<TaskCommentDto> AddCommentAsync(Guid taskId, AddTaskCommentDto dto, Guid userId);
    Task<IEnumerable<TaskCommentDto>> GetCommentsAsync(Guid taskId);
    Task<bool> ToggleWatcherAsync(Guid taskId, Guid userId);
    Task ReorderAsync(ReorderTasksDto dto);
    Task<IEnumerable<TaskResponseDto>> GetOverdueAsync(Guid chamberId);
    Task<TaskResponseDto> StartTimeTrackingAsync(Guid taskId);
    Task<TaskResponseDto> StopTimeTrackingAsync(Guid taskId, double minutes);
}
