using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class CaseActivity : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
}
