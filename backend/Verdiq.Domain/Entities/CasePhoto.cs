namespace Verdiq.Domain.Entities;

public class CasePhoto : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Caption { get; set; }
    public DateTime CapturedAt { get; set; }

    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
