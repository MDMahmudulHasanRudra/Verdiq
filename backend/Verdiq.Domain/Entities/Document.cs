using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public Guid CaseId { get; set; }
    public Guid UploadedById { get; set; }

    public string StorageProvider { get; set; } = "Local";
    public string? StorageKey { get; set; }
    public int CurrentVersion { get; set; } = 1;
    public Guid OrganizationId { get; set; }

    public Case Case { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentTag> Tags { get; set; } = new List<DocumentTag>();
}
