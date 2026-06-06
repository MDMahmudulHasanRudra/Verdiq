namespace Verdiq.Application.DTOs.Task;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string? Priority { get; set; }
    public Guid AssignedTo { get; set; }
    public Guid? CaseId { get; set; }
    public int SortOrder { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public double? EstimatedHours { get; set; }
    public List<Guid>? WatcherIds { get; set; }
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedTo { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
}

public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public Guid AssignedTo { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string AssignedByName { get; set; } = string.Empty;
    public Guid? CaseId { get; set; }
    public string? CaseTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SortOrder { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public List<TaskCommentDto> Comments { get; set; } = new();
    public List<TaskAttachmentDto> Attachments { get; set; } = new();
    public List<Guid> WatcherIds { get; set; } = new();
}

public class TaskCommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TaskAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddTaskCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class ReorderTasksDto
{
    public List<TaskOrderItem> Tasks { get; set; } = new();
}

public class TaskOrderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
    public string? Status { get; set; }
}
