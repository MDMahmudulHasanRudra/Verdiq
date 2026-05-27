using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Task;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, TaskResponseDto? Data)> CreateAsync(CreateTaskDto dto, Guid assignedBy, Guid chamberId)
    {
        var assignee = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.AssignedTo && u.ChamberId == chamberId && !u.IsDeleted);

        if (assignee == null)
            return (false, "Assigned user not found in this chamber", null);

        var taskEntity = new Domain.Entities.Task
        {
            Title = dto.Title,
            Description = dto.Description,
            DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc),
            Priority = dto.Priority,
            Status = Domain.Enums.TaskStatus.Pending,
            AssignedTo = dto.AssignedTo,
            AssignedBy = assignedBy,
            CaseId = dto.CaseId,
            ChamberId = chamberId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskEntity);
        await _context.SaveChangesAsync();

        return (true, "Task assigned successfully", await GetFullDtoAsync(taskEntity.Id));
    }

    public async Task<(bool Success, string Message, TaskResponseDto? Data)> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var taskEntity = await _context.Tasks.FindAsync(id);
        if (taskEntity == null || taskEntity.IsDeleted)
            return (false, "Task not found", null);

        if (dto.Title != null) taskEntity.Title = dto.Title;
        if (dto.Description != null) taskEntity.Description = dto.Description;
        if (dto.DueDate.HasValue) taskEntity.DueDate = DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc);
        if (dto.Priority != null) taskEntity.Priority = dto.Priority;
        if (dto.Status != null && Enum.TryParse<Domain.Enums.TaskStatus>(dto.Status, true, out var status))
            taskEntity.Status = status;

        taskEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Task updated successfully", await GetFullDtoAsync(id));
    }

    public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Where(t => t.AssignedTo == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetByCaseIdAsync(Guid caseId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Where(t => t.CaseId == caseId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid chamberId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    private async Task<TaskResponseDto?> GetFullDtoAsync(Guid id)
    {
        var taskEntity = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return taskEntity == null ? null : MapToDto(taskEntity);
    }

    private static TaskResponseDto MapToDto(Domain.Entities.Task t)
    {
        return new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            Status = t.Status.ToString(),
            Priority = t.Priority,
            AssignedTo = t.AssignedTo,
            AssignedToName = t.AssignedUser.FullName,
            AssignedByName = t.Assigner.FullName,
            CaseId = t.CaseId,
            CaseTitle = t.Case != null ? t.Case.Title : null,
            CreatedAt = t.CreatedAt
        };
    }
}
