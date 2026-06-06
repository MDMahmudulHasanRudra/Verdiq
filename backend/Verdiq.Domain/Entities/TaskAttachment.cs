namespace Verdiq.Domain.Entities;

public class TaskAttachment : BaseEntity
{
    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
