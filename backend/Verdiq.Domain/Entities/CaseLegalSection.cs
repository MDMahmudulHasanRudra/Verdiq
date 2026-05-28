namespace Verdiq.Domain.Entities;

public class CaseLegalSection : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public Guid LegalSectionId { get; set; }
    public LegalSection LegalSection { get; set; } = null!;

    public ICollection<CaseLegalProcedure> CaseProcedures { get; set; } = new List<CaseLegalProcedure>();
}
