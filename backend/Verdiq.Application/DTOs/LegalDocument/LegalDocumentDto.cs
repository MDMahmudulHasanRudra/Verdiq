namespace Verdiq.Application.DTOs.LegalDocument;

public class CreateLegalDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Citation { get; set; }
    public string? JudgeName { get; set; }
    public string? Keywords { get; set; }
    public int? Year { get; set; }
}

public class UpdateLegalDocumentDto
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Content { get; set; }
    public string? Citation { get; set; }
    public string? JudgeName { get; set; }
    public string? Keywords { get; set; }
    public int? Year { get; set; }
}

public class LegalDocumentResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Citation { get; set; }
    public string? JudgeName { get; set; }
    public string? Keywords { get; set; }
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; }
}
