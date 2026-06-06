namespace Verdiq.Application.DTOs.Banking;

public class CreateBankAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? RoutingNumber { get; set; }
    public string AccountType { get; set; } = "Savings";
    public decimal OpeningBalance { get; set; }
}

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? RoutingNumber { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBankTransactionDto
{
    public Guid BankAccountId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? Payee { get; set; }
    public string? Description { get; set; }
}

public class BankTransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? Payee { get; set; }
    public string? Description { get; set; }
    public string ReconciliationStatus { get; set; } = string.Empty;
    public DateTime? ReconciledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
