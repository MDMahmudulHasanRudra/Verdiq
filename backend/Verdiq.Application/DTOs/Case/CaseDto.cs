namespace Verdiq.Application.DTOs.Case;

public class CreateCaseDto
{
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public string? ActsAndSections { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public Guid ClientId { get; set; }
}

public class UpdateCaseDto
{
    public string? Title { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Court { get; set; }
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public string? ActsAndSections { get; set; }
    public string? Description { get; set; }
}

public class CaseResponseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public string? ActsAndSections { get; set; }
    public string? Description { get; set; }
    public DateTime FilingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid AssignedLawyerId { get; set; }
    public string AssignedLawyerName { get; set; } = string.Empty;
    public int DocumentsCount { get; set; }
    public int HearingsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
