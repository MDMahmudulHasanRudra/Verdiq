namespace Verdiq.Domain.Entities;

public class CauseList : BaseEntity
{
    public string CourtName { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public DateTime HearingDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
