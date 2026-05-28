namespace Verdiq.Domain.Entities;

public class WorkflowTemplate : BaseEntity
{
    public Guid ChamberId { get; set; }
    public virtual Chamber Chamber { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }

    public ICollection<WorkflowTemplateSection> Sections { get; set; } = new List<WorkflowTemplateSection>();
}
