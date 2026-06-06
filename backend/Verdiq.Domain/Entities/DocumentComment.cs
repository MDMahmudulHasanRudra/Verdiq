namespace Verdiq.Domain.Entities;

public class DocumentComment : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public DocumentComment? ParentComment { get; set; }
    public ICollection<DocumentComment> Replies { get; set; } = new List<DocumentComment>();
}
