namespace Verdiq.Domain.Entities;

public class WorkflowTemplateSection : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual WorkflowTemplate Template { get; set; } = null!;
    public Guid LegalSectionId { get; set; }
    public virtual LegalSection LegalSection { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
