namespace Verdiq.Application.DTOs.Case;

public class JudgmentDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Result { get; set; }
    public DateTime JudgmentDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? KeyFindings { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public bool HasDocument => !string.IsNullOrWhiteSpace(FileName);
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateJudgmentDto
{
    public string Caption { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Result { get; set; }
    public DateTime? JudgmentDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? KeyFindings { get; set; }
}
