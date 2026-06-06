namespace Verdiq.Domain.Entities;

public class JournalLine : BaseEntity
{
    public Guid JournalId { get; set; }
    public AccountingJournal Journal { get; set; } = null!;
    public Guid AccountId { get; set; }
    public ChartOfAccount Account { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
}
