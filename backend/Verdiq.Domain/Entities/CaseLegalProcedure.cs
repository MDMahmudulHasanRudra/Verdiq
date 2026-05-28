namespace Verdiq.Domain.Entities;

public class CaseLegalProcedure : BaseEntity
{
    public Guid CaseLegalSectionId { get; set; }
    public CaseLegalSection CaseLegalSection { get; set; } = null!;
    public Guid LegalProcedureId { get; set; }
    public LegalProcedure LegalProcedure { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
}
