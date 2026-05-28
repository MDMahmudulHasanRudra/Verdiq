namespace Verdiq.Application.DTOs.LegalProcedure;

public class CreateLegalProcedureDto
{
    public Guid LegalSectionId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsMandatory { get; set; }
}

public class UpdateLegalProcedureDto
{
    public int? StepNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool? IsMandatory { get; set; }
}

public class LegalProcedureResponseDto
{
    public Guid Id { get; set; }
    public Guid LegalSectionId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CaseLegalProcedureResponseDto
{
    public Guid Id { get; set; }
    public Guid CaseLegalSectionId { get; set; }
    public Guid LegalProcedureId { get; set; }
    public string ProcedureTitle { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
}
