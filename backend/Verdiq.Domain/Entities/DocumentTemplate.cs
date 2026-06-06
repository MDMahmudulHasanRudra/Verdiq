namespace Verdiq.Domain.Entities;

public class DocumentTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? StorageKey { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long FileSize { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = true;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
}
