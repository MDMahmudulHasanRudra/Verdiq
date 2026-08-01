namespace Verdiq.Domain.Entities;

public class Workflow : BaseEntity
{
    public Guid ChamberId { get; set; }
    public virtual Chamber Chamber { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;

    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<CaseWorkflow> CaseWorkflows { get; set; } = new List<CaseWorkflow>();
}
