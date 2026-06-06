namespace Verdiq.Domain.Entities;

public class DocumentFavorite : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
