using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Budget : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
}

public class BudgetLine : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;
    public Guid AccountId { get; set; }
    public ChartOfAccount Account { get; set; } = null!;
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
}
