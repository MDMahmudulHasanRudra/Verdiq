namespace Verdiq.Domain.Entities;

public class CaseWorkflow : BaseEntity
{
    public Guid CaseId { get; set; }
    public virtual Case Case { get; set; } = null!;

    public Guid WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    public string WorkflowName { get; set; } = string.Empty;
    public string? WorkflowDescription { get; set; }

    public string Status { get; set; } = "InProgress";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid StartedById { get; set; }
    public virtual User StartedBy { get; set; } = null!;

    public ICollection<CaseWorkflowStep> Steps { get; set; } = new List<CaseWorkflowStep>();
}
