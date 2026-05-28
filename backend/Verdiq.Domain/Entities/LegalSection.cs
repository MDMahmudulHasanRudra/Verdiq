namespace Verdiq.Domain.Entities;

public class LegalSection : BaseEntity
{
    public string SectionCode { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string LawName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<LegalProcedure> Procedures { get; set; } = new List<LegalProcedure>();
    public ICollection<CaseLegalSection> CaseLegalSections { get; set; } = new List<CaseLegalSection>();
}
