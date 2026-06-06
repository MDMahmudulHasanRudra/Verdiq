namespace Verdiq.Domain.Entities;

public class AccountingJournal : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}
