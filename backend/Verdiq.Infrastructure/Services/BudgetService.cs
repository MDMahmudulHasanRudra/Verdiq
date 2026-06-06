using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Budget;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly AppDbContext _context;
    public BudgetService(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> CreateBudgetAsync(CreateBudgetDto dto, Guid userId, Guid chamberId)
    {
        var budget = new Budget
        {
            Name = dto.Name, FiscalYear = dto.FiscalYear,
            TotalAmount = dto.TotalAmount, Description = dto.Description,
            ChamberId = chamberId, CreatedById = userId
        };

        foreach (var lineDto in dto.Lines)
        {
            budget.Lines.Add(new BudgetLine
            {
                BudgetId = budget.Id, AccountId = lineDto.AccountId,
                AllocatedAmount = lineDto.AllocatedAmount
            });
        }

        _context.Set<Budget>().Add(budget);
        await _context.SaveChangesAsync();
        return (await GetBudgetByIdAsync(budget.Id))!;
    }

    public async Task<BudgetResponseDto> ApproveBudgetAsync(Guid id)
    {
        var b = await _context.Set<Budget>().FindAsync(id)
            ?? throw new KeyNotFoundException("Budget not found");
        b.Status = BudgetStatus.Approved; b.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetBudgetByIdAsync(id))!;
    }

    public async Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid chamberId, int? fiscalYear)
    {
        var q = _context.Set<Budget>()
            .Include(b => b.Lines).ThenInclude(l => l.Account)
            .Include(b => b.CreatedBy)
            .Where(b => b.ChamberId == chamberId && !b.IsDeleted);
        if (fiscalYear.HasValue) q = q.Where(b => b.FiscalYear == fiscalYear.Value);
        return await q.OrderByDescending(b => b.FiscalYear).Select(b => MapBudget(b)).ToListAsync();
    }

    public async Task<BudgetResponseDto?> GetBudgetByIdAsync(Guid id)
    {
        var b = await _context.Set<Budget>()
            .Include(b => b.Lines).ThenInclude(l => l.Account)
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        return b == null ? null : MapBudget(b);
    }

    private static BudgetResponseDto MapBudget(Budget b) => new()
    {
        Id = b.Id, Name = b.Name, FiscalYear = b.FiscalYear,
        TotalAmount = b.TotalAmount,
        TotalSpent = b.Lines.Sum(l => l.SpentAmount),
        Remaining = b.TotalAmount - b.Lines.Sum(l => l.SpentAmount),
        Description = b.Description, Status = b.Status.ToString(),
        CreatedByName = b.CreatedBy?.FullName ?? "Unknown",
        CreatedAt = b.CreatedAt,
        Lines = b.Lines.Select(l => new BudgetLineResponseDto
        {
            Id = l.Id, AccountId = l.AccountId,
            AccountCode = l.Account?.Code ?? "",
            AccountName = l.Account?.Name ?? "",
            AllocatedAmount = l.AllocatedAmount,
            SpentAmount = l.SpentAmount,
            Remaining = l.AllocatedAmount - l.SpentAmount,
            UsagePercent = l.AllocatedAmount > 0
                ? Math.Round(l.SpentAmount / l.AllocatedAmount * 100, 2) : 0
        }).ToList()
    };
}
