using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class LegalDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public LegalDocumentCategory Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Citation { get; set; }
    public string? JudgeName { get; set; }
    public string? Keywords { get; set; }
    public int? Year { get; set; }
}
