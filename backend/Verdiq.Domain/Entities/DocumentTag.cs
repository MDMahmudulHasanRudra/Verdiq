namespace Verdiq.Domain.Entities;

public class DocumentTag : BaseEntity
{
    public string TagName { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }

    public Document Document { get; set; } = null!;
}
