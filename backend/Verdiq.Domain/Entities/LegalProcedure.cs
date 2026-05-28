namespace Verdiq.Domain.Entities;

public class LegalProcedure : BaseEntity
{
    public Guid LegalSectionId { get; set; }
    public LegalSection LegalSection { get; set; } = null!;
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsMandatory { get; set; }

    public ICollection<CaseLegalProcedure> CaseProcedures { get; set; } = new List<CaseLegalProcedure>();
}
