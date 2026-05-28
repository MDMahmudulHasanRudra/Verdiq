namespace Verdiq.Application.DTOs.LegalSection;

public class CreateLegalSectionDto
{
    public string SectionCode { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string LawName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
}

public class UpdateLegalSectionDto
{
    public string? SectionCode { get; set; }
    public string? SectionTitle { get; set; }
    public string? LawName { get; set; }
    public string? Country { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public bool? IsActive { get; set; }
}

public class LegalSectionResponseDto
{
    public Guid Id { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string LawName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public bool IsActive { get; set; }
    public int ProcedureCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
