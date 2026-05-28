using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FolderPath { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int Version { get; set; } = 1;

    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    public string StorageProvider { get; set; } = "Local";
    public string? StorageKey { get; set; }

    public string Visibility { get; set; } = "InternalOnly"; // InternalOnly, SharedWithClient
    public Guid? SharedWithClientId { get; set; }
    public Client? SharedWithClient { get; set; }

    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
