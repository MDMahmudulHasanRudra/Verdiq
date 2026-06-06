using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Banking;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class BankingService : IBankingService
{
    private readonly AppDbContext _context;
    public BankingService(AppDbContext context) => _context = context;

    public async Task<BankAccountResponseDto> CreateAccountAsync(CreateBankAccountDto dto, Guid chamberId)
    {
        var acc = new BankAccount
        {
            AccountName = dto.AccountName, BankName = dto.BankName,
            BranchName = dto.BranchName, AccountNumber = dto.AccountNumber,
            RoutingNumber = dto.RoutingNumber, AccountType = dto.AccountType,
            OpeningBalance = dto.OpeningBalance, CurrentBalance = dto.OpeningBalance,
            ChamberId = chamberId
        };
        _context.Set<BankAccount>().Add(acc);
        await _context.SaveChangesAsync();
        return MapAccount(acc);
    }

    public async Task<BankAccountResponseDto> UpdateAccountAsync(Guid id, CreateBankAccountDto dto)
    {
        var acc = await _context.Set<BankAccount>().FindAsync(id)
            ?? throw new KeyNotFoundException("Bank account not found");
        acc.AccountName = dto.AccountName; acc.BankName = dto.BankName;
        acc.BranchName = dto.BranchName; acc.AccountNumber = dto.AccountNumber;
        acc.RoutingNumber = dto.RoutingNumber; acc.AccountType = dto.AccountType;
        acc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapAccount(acc);
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var acc = await _context.Set<BankAccount>().FindAsync(id)
            ?? throw new KeyNotFoundException("Bank account not found");
        acc.IsDeleted = true; acc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<BankAccountResponseDto>> GetAccountsAsync(Guid chamberId)
    {
        return await _context.Set<BankAccount>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted)
            .OrderBy(a => a.BankName)
            .Select(a => MapAccount(a))
            .ToListAsync();
    }

    public async Task<BankAccountResponseDto?> GetAccountByIdAsync(Guid id)
    {
        var acc = await _context.Set<BankAccount>().FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        return acc == null ? null : MapAccount(acc);
    }

    public async Task<BankTransactionResponseDto> CreateTransactionAsync(CreateBankTransactionDto dto)
    {
        var acc = await _context.Set<BankAccount>().FindAsync(dto.BankAccountId)
            ?? throw new KeyNotFoundException("Bank account not found");

        var txn = new BankTransaction
        {
            BankAccountId = dto.BankAccountId, TransactionDate = dto.TransactionDate,
            TransactionType = dto.TransactionType, Amount = dto.Amount,
            ReferenceNo = dto.ReferenceNo, ChequeNo = dto.ChequeNo,
            Payee = dto.Payee, Description = dto.Description
        };

        acc.CurrentBalance += dto.TransactionType == "Deposit" ? dto.Amount : -dto.Amount;
        acc.UpdatedAt = DateTime.UtcNow;

        _context.Set<BankTransaction>().Add(txn);
        await _context.SaveChangesAsync();
        return MapTransaction(txn, acc.AccountName);
    }

    public async Task<List<BankTransactionResponseDto>> GetTransactionsAsync(Guid bankAccountId, int page, int pageSize)
    {
        return await _context.Set<BankTransaction>().Include(t => t.BankAccount)
            .Where(t => t.BankAccountId == bankAccountId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => MapTransaction(t, t.BankAccount.AccountName))
            .ToListAsync();
    }

    public async Task<BankTransactionResponseDto> ReconcileTransactionAsync(Guid id)
    {
        var txn = await _context.Set<BankTransaction>().Include(t => t.BankAccount)
            .FirstOrDefaultAsync(t => t.Id == id) ?? throw new KeyNotFoundException("Transaction not found");
        txn.ReconciliationStatus = ReconciliationStatus.Reconciled;
        txn.ReconciledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapTransaction(txn, txn.BankAccount.AccountName);
    }

    public async Task<BankAccountResponseDto> ReconcileAccountAsync(Guid accountId)
    {
        var acc = await _context.Set<BankAccount>().FirstOrDefaultAsync(a => a.Id == accountId)
            ?? throw new KeyNotFoundException("Bank account not found");
        var balance = await _context.Set<BankTransaction>()
            .Where(t => t.BankAccountId == accountId && !t.IsDeleted)
            .SumAsync(t => t.TransactionType == "Deposit" ? t.Amount : -t.Amount);
        acc.CurrentBalance = acc.OpeningBalance + balance;
        acc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapAccount(acc);
    }

    private static BankAccountResponseDto MapAccount(BankAccount a) => new()
    {
        Id = a.Id, AccountName = a.AccountName, BankName = a.BankName,
        BranchName = a.BranchName, AccountNumber = a.AccountNumber,
        RoutingNumber = a.RoutingNumber, AccountType = a.AccountType,
        OpeningBalance = a.OpeningBalance, CurrentBalance = a.CurrentBalance,
        IsActive = a.IsActive, CreatedAt = a.CreatedAt
    };

    private static BankTransactionResponseDto MapTransaction(BankTransaction t, string accName) => new()
    {
        Id = t.Id, BankAccountId = t.BankAccountId, BankAccountName = accName,
        TransactionDate = t.TransactionDate, TransactionType = t.TransactionType,
        Amount = t.Amount, ReferenceNo = t.ReferenceNo, ChequeNo = t.ChequeNo,
        Payee = t.Payee, Description = t.Description,
        ReconciliationStatus = t.ReconciliationStatus.ToString(),
        ReconciledAt = t.ReconciledAt, CreatedAt = t.CreatedAt
    };
}
