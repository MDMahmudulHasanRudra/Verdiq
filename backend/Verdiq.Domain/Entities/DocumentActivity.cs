namespace Verdiq.Domain.Entities;

public class DocumentActivity : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}
