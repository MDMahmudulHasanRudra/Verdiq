using Verdiq.Application.DTOs.Expense;

namespace Verdiq.Application.Interfaces;

public interface IExpenseService
{
    Task<(bool Success, string Message, ExpenseResponseDto? Data)> CreateAsync(CreateExpenseDto dto, Guid userId, Guid chamberId);
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(Guid chamberId, string? category = null, int page = 1, int pageSize = 10);
    Task<decimal> GetTotalAsync(Guid chamberId);
}
