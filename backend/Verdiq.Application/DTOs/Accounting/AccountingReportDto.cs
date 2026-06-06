namespace Verdiq.Application.DTOs.Accounting;

public class ProfitLossDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public List<ProfitLossCategoryDto> IncomeCategories { get; set; } = new();
    public List<ProfitLossCategoryDto> ExpenseCategories { get; set; } = new();
}

public class ProfitLossCategoryDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MonthlyReportDto
{
    public int Year { get; set; }
    public List<MonthlyReportItemDto> Months { get; set; } = new();
}

public class MonthlyReportItemDto
{
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
    public decimal CumulativeProfit { get; set; }
}

public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public List<BalanceSheetItemDto> Assets { get; set; } = new();
    public List<BalanceSheetItemDto> Liabilities { get; set; } = new();
    public List<BalanceSheetItemDto> Equity { get; set; } = new();
}

public class BalanceSheetItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
