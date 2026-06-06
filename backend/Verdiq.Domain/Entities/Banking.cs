using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class BankAccount : BaseEntity
{
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? RoutingNumber { get; set; }
    public string AccountType { get; set; } = "Savings";
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}

public class BankTransaction : BaseEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? Payee { get; set; }
    public string? Description { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; } = ReconciliationStatus.Unreconciled;
    public DateTime? ReconciledAt { get; set; }
}
