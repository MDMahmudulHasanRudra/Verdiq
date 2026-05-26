namespace Verdiq.Domain.Entities;

public class DocumentContent : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string ExtractedText { get; set; } = string.Empty;
}
