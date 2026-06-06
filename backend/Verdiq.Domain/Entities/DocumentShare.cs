namespace Verdiq.Domain.Entities;

public class DocumentShare : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid SharedWithUserId { get; set; }
    public User SharedWithUser { get; set; } = null!;
    public string Permissions { get; set; } = "View";
    public Guid SharedById { get; set; }
}
