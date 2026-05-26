namespace Verdiq.Application.DTOs.Case;

public class CreateCaseDto
{
    public string Title { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public string? Opponent { get; set; }
    public string? Priority { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public List<Guid> ClientIds { get; set; } = new();
}

public class UpdateCaseDto
{
    public string? Title { get; set; }
    public string? CourtName { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Opponent { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public List<Guid>? ClientIds { get; set; }
}

public class CaseResponseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public string? Opponent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid AssignedLawyerId { get; set; }
    public string AssignedLawyerName { get; set; } = string.Empty;
    public List<ClientInfo> Clients { get; set; } = new();
    public int HearingsCount { get; set; }
    public int DocumentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClientInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
