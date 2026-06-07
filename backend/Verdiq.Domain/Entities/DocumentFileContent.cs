namespace Verdiq.Domain.Entities;

public class DocumentFileContent : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
