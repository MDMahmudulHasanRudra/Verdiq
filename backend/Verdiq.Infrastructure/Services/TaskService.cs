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
            SortOrder = dto.SortOrder,
            IsRecurring = dto.IsRecurring,
            RecurrencePattern = dto.RecurrencePattern,
            RecurrenceInterval = dto.RecurrenceInterval,
            EstimatedHours = dto.EstimatedHours,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskEntity);

        if (dto.WatcherIds?.Count > 0)
        {
            foreach (var watcherId in dto.WatcherIds.Where(w => w != dto.AssignedTo))
            {
                _context.TaskWatchers.Add(new TaskWatcher
                {
                    TaskId = taskEntity.Id,
                    UserId = watcherId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return (true, "Task assigned successfully", await GetFullDtoAsync(taskEntity.Id));
    }

    public async Task<(bool Success, string Message, TaskResponseDto? Data)> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var taskEntity = await _context.Tasks
            .Include(t => t.Watchers)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (taskEntity == null)
            return (false, "Task not found", null);

        if (dto.Title != null) taskEntity.Title = dto.Title;
        if (dto.Description != null) taskEntity.Description = dto.Description;
        if (dto.DueDate.HasValue) taskEntity.DueDate = DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc);
        if (dto.Priority != null) taskEntity.Priority = dto.Priority;
        if (dto.AssignedTo.HasValue) taskEntity.AssignedTo = dto.AssignedTo.Value;
        if (dto.SortOrder.HasValue) taskEntity.SortOrder = dto.SortOrder.Value;
        if (dto.IsRecurring.HasValue) taskEntity.IsRecurring = dto.IsRecurring.Value;
        if (dto.RecurrencePattern != null) taskEntity.RecurrencePattern = dto.RecurrencePattern;
        if (dto.RecurrenceInterval.HasValue) taskEntity.RecurrenceInterval = dto.RecurrenceInterval;
        if (dto.EstimatedHours.HasValue) taskEntity.EstimatedHours = dto.EstimatedHours;
        if (dto.ActualHours.HasValue) taskEntity.ActualHours = dto.ActualHours;

        if (dto.Status != null && Enum.TryParse<Domain.Enums.TaskStatus>(dto.Status, true, out var status))
        {
            taskEntity.Status = status;
            if (status == Domain.Enums.TaskStatus.Completed)
                taskEntity.CompletedAt = DateTime.UtcNow;
        }

        taskEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Task updated successfully", await GetFullDtoAsync(id));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var taskEntity = await _context.Tasks.FindAsync(id);
        if (taskEntity == null || taskEntity.IsDeleted)
            return (false, "Task not found");

        taskEntity.IsDeleted = true;
        taskEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Task deleted successfully");
    }

    public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Watchers)
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
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Watchers)
            .Where(t => t.CaseId == caseId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? priority = null, Guid? assignedTo = null)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Watchers)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.TaskStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority != null && t.Priority.ToLower() == priority.ToLower());

        if (assignedTo.HasValue)
            query = query.Where(t => t.AssignedTo == assignedTo.Value);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        var taskEntity = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .Include(t => t.Watchers)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return taskEntity == null ? null : MapToDto(taskEntity);
    }

    public async Task<TaskCommentDto> AddCommentAsync(Guid taskId, AddTaskCommentDto dto, Guid userId)
    {
        var comment = new TaskComment
        {
            TaskId = taskId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return new TaskCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserId = userId,
            UserName = user?.FullName ?? "Unknown",
            UserAvatar = user?.AvatarUrl,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IEnumerable<TaskCommentDto>> GetCommentsAsync(Guid taskId)
    {
        return await _context.TaskComments
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new TaskCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                UserId = c.UserId,
                UserName = c.User.FullName,
                UserAvatar = c.User.AvatarUrl,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> ToggleWatcherAsync(Guid taskId, Guid userId)
    {
        var existing = await _context.TaskWatchers
            .FirstOrDefaultAsync(w => w.TaskId == taskId && w.UserId == userId);

        if (existing != null)
        {
            _context.TaskWatchers.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }

        _context.TaskWatchers.Add(new TaskWatcher
        {
            TaskId = taskId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ReorderAsync(ReorderTasksDto dto)
    {
        foreach (var item in dto.Tasks)
        {
            var task = await _context.Tasks.FindAsync(item.Id);
            if (task == null || task.IsDeleted) continue;

            task.SortOrder = item.SortOrder;
            if (item.Status != null && Enum.TryParse<Domain.Enums.TaskStatus>(item.Status, true, out var status))
                task.Status = status;

            task.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskResponseDto>> GetOverdueAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var tasks = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Watchers)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted
                && t.Status != Domain.Enums.TaskStatus.Completed
                && t.Status != Domain.Enums.TaskStatus.Cancelled
                && t.DueDate < now)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskResponseDto> StartTimeTrackingAsync(Guid taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found");

        task.Status = Domain.Enums.TaskStatus.InProgress;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetFullDtoAsync(taskId))!;
    }

    public async Task<TaskResponseDto> StopTimeTrackingAsync(Guid taskId, double minutes)
    {
        var task = await _context.Tasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found");

        task.ActualHours = (task.ActualHours ?? 0) + (minutes / 60.0);
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetFullDtoAsync(taskId))!;
    }

    private async Task<TaskResponseDto?> GetFullDtoAsync(Guid id)
    {
        var taskEntity = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .Include(t => t.Watchers)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return taskEntity == null ? null : MapToDto(taskEntity);
    }

    private TaskResponseDto MapToDto(Domain.Entities.Task t)
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
            CaseTitle = t.Case?.Title,
            CreatedAt = t.CreatedAt,
            SortOrder = t.SortOrder,
            IsRecurring = t.IsRecurring,
            RecurrencePattern = t.RecurrencePattern,
            RecurrenceInterval = t.RecurrenceInterval,
            CompletedAt = t.CompletedAt,
            EstimatedHours = t.EstimatedHours,
            ActualHours = t.ActualHours,
            CommentCount = t.Comments?.Count ?? 0,
            AttachmentCount = t.Attachments?.Count ?? 0,
            Comments = t.Comments?
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new TaskCommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.UserId,
                    UserName = c.User?.FullName ?? "Unknown",
                    UserAvatar = c.User?.AvatarUrl,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<TaskCommentDto>(),
            Attachments = t.Attachments?
                .Where(a => !a.IsDeleted)
                .Select(a => new TaskAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    UploadedByName = a.UploadedBy?.FullName ?? "Unknown",
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<TaskAttachmentDto>(),
            WatcherIds = t.Watchers?
                .Where(w => !w.IsDeleted)
                .Select(w => w.UserId)
                .ToList() ?? new List<Guid>()
        };
    }
}
