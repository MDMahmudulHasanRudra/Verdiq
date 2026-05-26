using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class DocumentVersion : BaseEntity
{
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ChangeNotes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string StorageProvider { get; set; } = "Local";
    public string? StorageKey { get; set; }

    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
