namespace Verdiq.Application.DTOs.Workflow;

public class CreateWorkflowTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public List<Guid> LegalSectionIds { get; set; } = new();
}

public class UpdateWorkflowTemplateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDefault { get; set; }
    public List<Guid>? LegalSectionIds { get; set; }
}

public class WorkflowTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public List<WorkflowSectionItem> Sections { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class WorkflowSectionItem
{
    public Guid Id { get; set; }
    public Guid LegalSectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string LawName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
