namespace Verdiq.Application.DTOs.Task;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string? Priority { get; set; }
    public Guid AssignedTo { get; set; }
    public Guid? CaseId { get; set; }
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
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
}
