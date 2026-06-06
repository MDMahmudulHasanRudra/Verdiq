namespace Verdiq.Application.DTOs.Accounting;

public class AccountingDashboardDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int PendingInvoices { get; set; }
    public decimal PendingAmount { get; set; }
    public List<MonthlyFinanceDto> MonthlyTrend { get; set; } = new();
    public List<AccountBalanceDto> TopAccounts { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
}

public class MonthlyFinanceDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Profit { get; set; }
}

public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class RecentTransactionDto
{
    public Guid JournalId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public decimal Amount { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}
