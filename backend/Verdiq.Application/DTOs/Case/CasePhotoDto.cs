namespace Verdiq.Application.DTOs.Case;

public class CasePhotoDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Caption { get; set; }
    public DateTime CapturedAt { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadCasePhotoDto
{
    public string? Caption { get; set; }
}
