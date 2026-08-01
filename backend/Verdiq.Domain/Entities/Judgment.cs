namespace Verdiq.Domain.Entities;

public class Judgment : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public string Caption { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Result { get; set; }
    public DateTime JudgmentDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? KeyFindings { get; set; }

    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }

    public Guid RecordedById { get; set; }
    public User RecordedBy { get; set; } = null!;
}
