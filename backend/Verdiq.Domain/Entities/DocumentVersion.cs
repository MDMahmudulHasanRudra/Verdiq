using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class DocumentVersion : BaseEntity
{
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = "Local";
    public string? StorageKey { get; set; }
    public string? ChangeNotes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public Guid DocumentId { get; set; }
    public Guid UploadedById { get; set; }

    public Document Document { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
