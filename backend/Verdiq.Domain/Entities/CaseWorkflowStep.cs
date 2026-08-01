namespace Verdiq.Domain.Entities;

public class CaseWorkflowStep : BaseEntity
{
    public Guid CaseWorkflowId { get; set; }
    public virtual CaseWorkflow CaseWorkflow { get; set; } = null!;

    public Guid? StepId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DueInDays { get; set; }
    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = "Pending";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid? CompletedById { get; set; }
    public virtual User? CompletedBy { get; set; }

    public string? Notes { get; set; }
}
