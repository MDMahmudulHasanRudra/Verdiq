using Verdiq.Application.DTOs.Budget;

namespace Verdiq.Application.Interfaces;

public interface IBudgetService
{
    Task<BudgetResponseDto> CreateBudgetAsync(CreateBudgetDto dto, Guid userId, Guid chamberId);
    Task<BudgetResponseDto> ApproveBudgetAsync(Guid id);
    Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid chamberId, int? fiscalYear);
    Task<BudgetResponseDto?> GetBudgetByIdAsync(Guid id);
}
