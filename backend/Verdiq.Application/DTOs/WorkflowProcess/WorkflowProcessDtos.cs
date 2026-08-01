namespace Verdiq.Application.DTOs.WorkflowProcess;

public class WorkflowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int StepCount { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkflowStepItemDto> Steps { get; set; } = new();
}

public class WorkflowStepItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DueInDays { get; set; }
}

public class CreateWorkflowStepDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DueInDays { get; set; }
}

public class CreateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateWorkflowStepDto> Steps { get; set; } = new();
}

public class UpdateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateWorkflowStepDto> Steps { get; set; } = new();
}

public class CaseWorkflowDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public Guid WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string? WorkflowDescription { get; set; }
    public string Status { get; set; } = "InProgress";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StartedByName { get; set; }
    public int StepCount { get; set; }
    public int CompletedStepCount { get; set; }
    public int PercentComplete { get; set; }
    public bool IsOverdue { get; set; }
    public string? NextStepTitle { get; set; }
    public List<CaseWorkflowStepDto> Steps { get; set; } = new();
}

public class CaseWorkflowStepDto
{
    public Guid Id { get; set; }
    public Guid? StepId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOverdue { get; set; }
}

public class LinkWorkflowDto
{
    public Guid WorkflowId { get; set; }
}

public class CompleteStepDto
{
    public string? Notes { get; set; }
}
