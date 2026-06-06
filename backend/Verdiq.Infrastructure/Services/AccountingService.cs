using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Accounting;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly AppDbContext _context;

    public AccountingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JournalResponseDto> CreateJournalAsync(CreateJournalDto dto, Guid userId, Guid chamberId)
    {
        if (!dto.Lines.Any())
            throw new InvalidOperationException("Journal must have at least one line");

        var totalDebit = dto.Lines.Sum(l => l.DebitAmount);
        var totalCredit = dto.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
            throw new InvalidOperationException($"Debit ({totalDebit}) and Credit ({totalCredit}) must be equal");

        var count = await _context.Set<AccountingJournal>()
            .Where(j => j.ChamberId == chamberId).CountAsync();

        var journal = new AccountingJournal
        {
            EntryNumber = $"JV-{DateTime.UtcNow:yyyyMM}-{count + 1:D4}",
            EntryDate = dto.EntryDate == default ? DateTime.UtcNow : dto.EntryDate,
            Description = dto.Description,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            ChamberId = chamberId,
            CreatedById = userId
        };

        foreach (var lineDto in dto.Lines)
        {
            var account = await _context.Set<ChartOfAccount>().FindAsync(lineDto.AccountId)
                ?? throw new KeyNotFoundException($"Account {lineDto.AccountId} not found");

            journal.Lines.Add(new JournalLine
            {
                JournalId = journal.Id,
                AccountId = lineDto.AccountId,
                DebitAmount = lineDto.DebitAmount,
                CreditAmount = lineDto.CreditAmount,
                Description = lineDto.Description
            });
        }

        _context.Set<AccountingJournal>().Add(journal);
        await _context.SaveChangesAsync();
        return (await GetJournalByIdAsync(journal.Id))!;
    }

    public async Task<JournalResponseDto> UpdateJournalAsync(Guid id, CreateJournalDto dto)
    {
        var journal = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted)
            ?? throw new KeyNotFoundException("Journal not found");

        var totalDebit = dto.Lines.Sum(l => l.DebitAmount);
        var totalCredit = dto.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
            throw new InvalidOperationException("Debit and Credit must be equal");

        journal.EntryDate = dto.EntryDate;
        journal.Description = dto.Description;
        journal.ReferenceType = dto.ReferenceType;
        journal.ReferenceId = dto.ReferenceId;
        journal.UpdatedAt = DateTime.UtcNow;

        foreach (var line in journal.Lines)
            line.IsDeleted = true;

        foreach (var lineDto in dto.Lines)
        {
            journal.Lines.Add(new JournalLine
            {
                JournalId = journal.Id,
                AccountId = lineDto.AccountId,
                DebitAmount = lineDto.DebitAmount,
                CreditAmount = lineDto.CreditAmount,
                Description = lineDto.Description
            });
        }

        await _context.SaveChangesAsync();
        return (await GetJournalByIdAsync(id))!;
    }

    public async Task DeleteJournalAsync(Guid id)
    {
        var journal = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted)
            ?? throw new KeyNotFoundException("Journal not found");

        journal.IsDeleted = true;
        journal.UpdatedAt = DateTime.UtcNow;
        foreach (var line in journal.Lines)
            line.IsDeleted = true;

        await _context.SaveChangesAsync();
    }

    public async Task<JournalResponseDto?> GetJournalByIdAsync(Guid id)
    {
        var journal = await _context.Set<AccountingJournal>()
            .Include(j => j.CreatedBy)
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);

        return journal == null ? null : MapJournalToDto(journal);
    }

    public async Task<(List<JournalResponseDto> Items, int TotalCount)> GetJournalsAsync(
        Guid chamberId, int page = 1, int pageSize = 20,
        DateTime? from = null, DateTime? to = null, Guid? accountId = null)
    {
        var query = _context.Set<AccountingJournal>()
            .Include(j => j.CreatedBy)
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .Where(j => j.ChamberId == chamberId && !j.IsDeleted);

        if (from.HasValue) query = query.Where(j => j.EntryDate >= from.Value);
        if (to.HasValue) query = query.Where(j => j.EntryDate <= to.Value);
        if (accountId.HasValue)
            query = query.Where(j => j.Lines.Any(l => l.AccountId == accountId.Value));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items.Select(MapJournalToDto).ToList(), totalCount);
    }

    public async Task<AccountingDashboardDto> GetDashboardAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var journals = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .Include(j => j.CreatedBy)
            .Where(j => j.ChamberId == chamberId && !j.IsDeleted)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();

        var incomeAccounts = await _context.Set<ChartOfAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Income)
            .Select(a => a.Id)
            .ToListAsync();

        var expenseAccounts = await _context.Set<ChartOfAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Expense)
            .Select(a => a.Id)
            .ToListAsync();

        var thisMonthLines = journals
            .Where(j => j.EntryDate >= startOfMonth && j.EntryDate <= endOfMonth)
            .SelectMany(j => j.Lines)
            .ToList();

        var allLines = journals.SelectMany(j => j.Lines).ToList();

        var totalIncome = allLines.Where(l => incomeAccounts.Contains(l.AccountId)).Sum(l => l.CreditAmount - l.DebitAmount);
        var totalExpenses = allLines.Where(l => expenseAccounts.Contains(l.AccountId)).Sum(l => l.DebitAmount - l.CreditAmount);

        var monthlyData = new List<MonthlyFinanceDto>();
        for (var m = 1; m <= 12; m++)
        {
            var monthStart = new DateTime(now.Year, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthLines = journals
                .Where(j => j.EntryDate >= monthStart && j.EntryDate <= monthEnd)
                .SelectMany(j => j.Lines)
                .ToList();

            var monthIncome = monthLines.Where(l => incomeAccounts.Contains(l.AccountId)).Sum(l => l.CreditAmount - l.DebitAmount);
            var monthExpense = monthLines.Where(l => expenseAccounts.Contains(l.AccountId)).Sum(l => l.DebitAmount - l.CreditAmount);

            monthlyData.Add(new MonthlyFinanceDto
            {
                Year = now.Year,
                Month = m,
                Label = new DateTime(now.Year, m, 1).ToString("MMM"),
                Income = monthIncome,
                Expense = monthExpense,
                Profit = monthIncome - monthExpense
            });
        }

        var allAccounts = await _context.Set<ChartOfAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted)
            .ToListAsync();

        var topAccounts = new List<AccountBalanceDto>();
        foreach (var acc in allAccounts.OrderBy(a => a.Code).Take(10))
        {
            var accLines = allLines.Where(l => l.AccountId == acc.Id).ToList();
            var bal = CalculateBalance(acc, accLines.Sum(l => l.DebitAmount), accLines.Sum(l => l.CreditAmount));
            topAccounts.Add(new AccountBalanceDto
            {
                AccountId = acc.Id,
                AccountCode = acc.Code,
                AccountName = acc.Name,
                Type = acc.Type.ToString(),
                Balance = bal
            });
        }

        var pendingInvoices = await _context.Set<Invoice>()
            .Include(i => i.Client)
            .CountAsync(i => i.Client.ChamberId == chamberId && i.Status == Domain.Enums.PaymentStatus.Pending && !i.IsDeleted);

        var pendingAmount = await _context.Set<Invoice>()
            .Include(i => i.Client)
            .Where(i => i.Client.ChamberId == chamberId && i.Status == Domain.Enums.PaymentStatus.Pending && !i.IsDeleted)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var recentJournals = journals.Take(10).Select(j => new RecentTransactionDto
        {
            JournalId = j.Id,
            EntryNumber = j.EntryNumber,
            EntryDate = j.EntryDate,
            Description = j.Description,
            ReferenceType = j.ReferenceType,
            Amount = j.Lines.Sum(l => l.DebitAmount),
            CreatedByName = j.CreatedBy?.FullName ?? "Unknown"
        }).ToList();

        return new AccountingDashboardDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetProfit = totalIncome - totalExpenses,
            TotalRevenue = totalIncome,
            TotalTransactions = journals.Count,
            PendingInvoices = pendingInvoices,
            PendingAmount = pendingAmount,
            MonthlyTrend = monthlyData,
            TopAccounts = topAccounts,
            RecentTransactions = recentJournals
        };
    }

    public async Task<ProfitLossDto> GetProfitLossAsync(Guid chamberId, DateTime from, DateTime to)
    {
        var journals = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .Where(j => j.ChamberId == chamberId && !j.IsDeleted && j.EntryDate >= from && j.EntryDate <= to)
            .ToListAsync();

        var allLines = journals.SelectMany(j => j.Lines).ToList();

        var incomeAccounts = await _context.Set<ChartOfAccount>()
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Income && a.ParentId == null)
            .ToListAsync();

        var expenseAccounts = await _context.Set<ChartOfAccount>()
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Expense && a.ParentId == null)
            .ToListAsync();

        var incomeCategories = new List<ProfitLossCategoryDto>();
        foreach (var acc in incomeAccounts)
        {
            var ids = await GetAccountAndChildrenIds(acc.Id);
            var amount = allLines.Where(l => ids.Contains(l.AccountId)).Sum(l => l.CreditAmount - l.DebitAmount);
            if (amount != 0)
                incomeCategories.Add(new ProfitLossCategoryDto { AccountId = acc.Id, AccountCode = acc.Code, AccountName = acc.Name, Amount = amount });
        }

        var expenseCategories = new List<ProfitLossCategoryDto>();
        foreach (var acc in expenseAccounts)
        {
            var ids = await GetAccountAndChildrenIds(acc.Id);
            var amount = allLines.Where(l => ids.Contains(l.AccountId)).Sum(l => l.DebitAmount - l.CreditAmount);
            if (amount != 0)
                expenseCategories.Add(new ProfitLossCategoryDto { AccountId = acc.Id, AccountCode = acc.Code, AccountName = acc.Name, Amount = amount });
        }

        var totalIncome = incomeCategories.Sum(c => c.Amount);
        var totalExpenses = expenseCategories.Sum(c => c.Amount);

        return new ProfitLossDto
        {
            FromDate = from,
            ToDate = to,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetProfit = totalIncome - totalExpenses,
            IncomeCategories = incomeCategories,
            ExpenseCategories = expenseCategories
        };
    }

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(Guid chamberId, int year)
    {
        var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfYear = startOfYear.AddYears(1).AddDays(-1);

        var journals = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .Where(j => j.ChamberId == chamberId && !j.IsDeleted && j.EntryDate >= startOfYear && j.EntryDate <= endOfYear)
            .ToListAsync();

        var incomeIds = await _context.Set<ChartOfAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Income)
            .Select(a => a.Id).ToListAsync();

        var expenseIds = await _context.Set<ChartOfAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.Type == Domain.Enums.AccountType.Expense)
            .Select(a => a.Id).ToListAsync();

        var months = new List<MonthlyReportItemDto>();
        decimal cumulative = 0;

        for (var m = 1; m <= 12; m++)
        {
            var monthStart = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthLines = journals
                .Where(j => j.EntryDate >= monthStart && j.EntryDate <= monthEnd)
                .SelectMany(j => j.Lines).ToList();

            var income = monthLines.Where(l => incomeIds.Contains(l.AccountId)).Sum(l => l.CreditAmount - l.DebitAmount);
            var expenses = monthLines.Where(l => expenseIds.Contains(l.AccountId)).Sum(l => l.DebitAmount - l.CreditAmount);
            var profit = income - expenses;
            cumulative += profit;

            months.Add(new MonthlyReportItemDto
            {
                Month = m,
                Label = monthStart.ToString("MMM"),
                Income = income,
                Expenses = expenses,
                Profit = profit,
                CumulativeProfit = cumulative
            });
        }

        return new MonthlyReportDto { Year = year, Months = months };
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(Guid chamberId, DateTime asOfDate)
    {
        var journals = await _context.Set<AccountingJournal>()
            .Include(j => j.Lines.Where(l => !l.IsDeleted))
                .ThenInclude(l => l.Account)
            .Where(j => j.ChamberId == chamberId && !j.IsDeleted && j.EntryDate <= asOfDate)
            .ToListAsync();

        var allLines = journals.SelectMany(j => j.Lines).ToList();

        var accounts = await _context.Set<ChartOfAccount>()
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted && a.ParentId == null)
            .ToListAsync();

        var assets = new List<BalanceSheetItemDto>();
        var liabilities = new List<BalanceSheetItemDto>();
        var equity = new List<BalanceSheetItemDto>();
        decimal totalAssets = 0, totalLiabilities = 0, totalEquity = 0;

        foreach (var acc in accounts)
        {
            var ids = await GetAccountAndChildrenIds(acc.Id);
            var debitSum = allLines.Where(l => ids.Contains(l.AccountId)).Sum(l => l.DebitAmount);
            var creditSum = allLines.Where(l => ids.Contains(l.AccountId)).Sum(l => l.CreditAmount);
            var balance = CalculateBalance(acc, debitSum, creditSum);

            if (balance == 0) continue;

            var item = new BalanceSheetItemDto
            {
                AccountId = acc.Id,
                AccountCode = acc.Code,
                AccountName = acc.Name,
                Balance = balance
            };

            switch (acc.Type)
            {
                case Domain.Enums.AccountType.Asset:
                    assets.Add(item); totalAssets += balance; break;
                case Domain.Enums.AccountType.Liability:
                    liabilities.Add(item); totalLiabilities += balance; break;
                case Domain.Enums.AccountType.Equity:
                    equity.Add(item); totalEquity += balance; break;
            }
        }

        return new BalanceSheetDto
        {
            AsOfDate = asOfDate,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity
        };
    }

    private async Task<List<Guid>> GetAccountAndChildrenIds(Guid accountId)
    {
        var ids = new List<Guid> { accountId };
        var children = await _context.Set<ChartOfAccount>()
            .Where(a => a.ParentId == accountId && !a.IsDeleted)
            .Select(a => a.Id).ToListAsync();

        foreach (var childId in children)
            ids.AddRange(await GetAccountAndChildrenIds(childId));

        return ids;
    }

    private static decimal CalculateBalance(ChartOfAccount account, decimal debitSum, decimal creditSum)
    {
        return account.Type switch
        {
            Domain.Enums.AccountType.Asset or Domain.Enums.AccountType.Expense
                => account.OpeningBalance + debitSum - creditSum,
            Domain.Enums.AccountType.Liability or Domain.Enums.AccountType.Equity or Domain.Enums.AccountType.Income
                => account.OpeningBalance + creditSum - debitSum,
            _ => 0
        };
    }

    private static JournalResponseDto MapJournalToDto(AccountingJournal j)
    {
        return new JournalResponseDto
        {
            Id = j.Id,
            EntryNumber = j.EntryNumber,
            EntryDate = j.EntryDate,
            Description = j.Description,
            ReferenceType = j.ReferenceType,
            ReferenceId = j.ReferenceId,
            CreatedByName = j.CreatedBy?.FullName ?? "Unknown",
            CreatedAt = j.CreatedAt,
            TotalDebit = j.Lines?.Where(l => !l.IsDeleted).Sum(l => l.DebitAmount) ?? 0,
            TotalCredit = j.Lines?.Where(l => !l.IsDeleted).Sum(l => l.CreditAmount) ?? 0,
            Lines = j.Lines?
                .Where(l => !l.IsDeleted)
                .Select(l => new JournalLineResponseDto
                {
                    Id = l.Id,
                    AccountId = l.AccountId,
                    AccountCode = l.Account?.Code ?? "",
                    AccountName = l.Account?.Name ?? "",
                    AccountType = l.Account?.Type.ToString() ?? "",
                    DebitAmount = l.DebitAmount,
                    CreditAmount = l.CreditAmount,
                    Description = l.Description
                }).ToList() ?? new()
        };
    }
}
