using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Accounting;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class ChartOfAccountService : IChartOfAccountService
{
    private readonly AppDbContext _context;

    public ChartOfAccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AccountResponseDto> CreateAsync(CreateAccountDto dto, Guid chamberId)
    {
        if (dto.ParentId.HasValue)
        {
            var parent = await _context.Set<ChartOfAccount>().FindAsync(dto.ParentId.Value);
            if (parent == null || parent.ChamberId != chamberId)
                throw new KeyNotFoundException("Parent account not found");
        }

        var account = new ChartOfAccount
        {
            Code = dto.Code,
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            ParentId = dto.ParentId,
            OpeningBalance = dto.OpeningBalance,
            ChamberId = chamberId
        };

        _context.Set<ChartOfAccount>().Add(account);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(account.Id))!;
    }

    public async Task<AccountResponseDto> UpdateAsync(Guid id, UpdateAccountDto dto)
    {
        var account = await _context.Set<ChartOfAccount>().FindAsync(id)
            ?? throw new KeyNotFoundException("Account not found");

        if (dto.Code != null) account.Code = dto.Code;
        if (dto.Name != null) account.Name = dto.Name;
        if (dto.Type.HasValue) account.Type = dto.Type.Value;
        if (dto.Description != null) account.Description = dto.Description;
        if (dto.IsActive.HasValue) account.IsActive = dto.IsActive.Value;
        if (dto.OpeningBalance.HasValue) account.OpeningBalance = dto.OpeningBalance.Value;
        if (dto.ParentId != null) account.ParentId = dto.ParentId;

        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await _context.Set<ChartOfAccount>()
            .Include(a => a.Children)
            .Include(a => a.JournalLines)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted)
            ?? throw new KeyNotFoundException("Account not found");

        if (account.Children.Any(c => !c.IsDeleted))
            throw new InvalidOperationException("Cannot delete account with active sub-accounts");

        if (account.JournalLines.Any())
            throw new InvalidOperationException("Cannot delete account with journal entries");

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<AccountResponseDto?> GetByIdAsync(Guid id)
    {
        var account = await _context.Set<ChartOfAccount>()
            .Include(a => a.Parent)
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (account == null) return null;

        var balance = await CalculateBalanceAsync(account.Id);
        return MapToDto(account, balance);
    }

    public async Task<List<AccountResponseDto>> GetAllAsync(Guid chamberId)
    {
        var accounts = await _context.Set<ChartOfAccount>()
            .Include(a => a.Parent)
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync();

        var balances = new Dictionary<Guid, decimal>();
        foreach (var acc in accounts)
            balances[acc.Id] = await CalculateBalanceAsync(acc.Id);

        return accounts.Select(a => MapToDto(a, balances.GetValueOrDefault(a.Id))).ToList();
    }

    public async Task<List<AccountResponseDto>> GetTreeAsync(Guid chamberId)
    {
        var all = await GetAllAsync(chamberId);
        return BuildTree(all, null);
    }

    private static List<AccountResponseDto> BuildTree(List<AccountResponseDto> accounts, Guid? parentId)
    {
        return accounts
            .Where(a => a.ParentId == parentId)
            .Select(a =>
            {
                a.Children = BuildTree(accounts, a.Id);
                return a;
            })
            .ToList();
    }

    private async Task<decimal> CalculateBalanceAsync(Guid accountId)
    {
        var account = await _context.Set<ChartOfAccount>().FindAsync(accountId);
        if (account == null) return 0;

        var debitSum = await _context.Set<JournalLine>()
            .Where(l => l.AccountId == accountId && !l.IsDeleted)
            .SumAsync(l => (decimal?)l.DebitAmount) ?? 0;

        var creditSum = await _context.Set<JournalLine>()
            .Where(l => l.AccountId == accountId && !l.IsDeleted)
            .SumAsync(l => (decimal?)l.CreditAmount) ?? 0;

        var balance = account.Type switch
        {
            Domain.Enums.AccountType.Asset or Domain.Enums.AccountType.Expense
                => account.OpeningBalance + debitSum - creditSum,
            Domain.Enums.AccountType.Liability or Domain.Enums.AccountType.Equity or Domain.Enums.AccountType.Income
                => account.OpeningBalance + creditSum - debitSum,
            _ => 0
        };

        return balance;
    }

    private static AccountResponseDto MapToDto(ChartOfAccount a, decimal balance)
    {
        return new AccountResponseDto
        {
            Id = a.Id,
            Code = a.Code,
            Name = a.Name,
            Type = a.Type.ToString(),
            Description = a.Description,
            ParentId = a.ParentId,
            ParentName = a.Parent?.Name,
            IsActive = a.IsActive,
            OpeningBalance = a.OpeningBalance,
            Balance = balance,
            Children = a.Children?.Where(c => !c.IsDeleted).Select(c => new AccountResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Type = c.Type.ToString(),
                IsActive = c.IsActive
            }).ToList() ?? new()
        };
    }
}
