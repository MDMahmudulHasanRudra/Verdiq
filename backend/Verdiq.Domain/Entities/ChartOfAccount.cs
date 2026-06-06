using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class ChartOfAccount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public ChartOfAccount? Parent { get; set; }
    public ICollection<ChartOfAccount> Children { get; set; } = new List<ChartOfAccount>();
    public bool IsActive { get; set; } = true;
    public decimal OpeningBalance { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();
}
