namespace Verdiq.Domain.Entities;

public class WorkflowStep : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DueInDays { get; set; }
    public bool IsRequired { get; set; } = true;
}
