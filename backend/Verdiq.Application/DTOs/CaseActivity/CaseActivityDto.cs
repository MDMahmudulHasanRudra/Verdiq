namespace Verdiq.Application.DTOs.CaseActivity;

public class CaseActivityResponseDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
