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

    public Case Case { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
