using Verdiq.Application.DTOs.Banking;

namespace Verdiq.Application.Interfaces;

public interface IBankingService
{
    Task<BankAccountResponseDto> CreateAccountAsync(CreateBankAccountDto dto, Guid chamberId);
    Task<BankAccountResponseDto> UpdateAccountAsync(Guid id, CreateBankAccountDto dto);
    Task DeleteAccountAsync(Guid id);
    Task<List<BankAccountResponseDto>> GetAccountsAsync(Guid chamberId);
    Task<BankAccountResponseDto?> GetAccountByIdAsync(Guid id);
    Task<BankTransactionResponseDto> CreateTransactionAsync(CreateBankTransactionDto dto);
    Task<List<BankTransactionResponseDto>> GetTransactionsAsync(Guid bankAccountId, int page = 1, int pageSize = 20);
    Task<BankTransactionResponseDto> ReconcileTransactionAsync(Guid id);
    Task<BankAccountResponseDto> ReconcileAccountAsync(Guid accountId);
}
