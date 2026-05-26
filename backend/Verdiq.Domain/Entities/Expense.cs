namespace Verdiq.Domain.Entities;

public class Expense : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string Category { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptPath { get; set; }

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid? CaseId { get; set; }
    public Case? Case { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
