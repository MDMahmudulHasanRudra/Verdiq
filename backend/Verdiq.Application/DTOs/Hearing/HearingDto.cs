namespace Verdiq.Application.DTOs.Hearing;

public class CreateHearingDto
{
    public Guid CaseId { get; set; }
    public DateTime HearingDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string HearingType { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateHearingDto
{
    public DateTime? HearingDate { get; set; }
    public string? Time { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class HearingResponseDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public DateTime HearingDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string HearingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
