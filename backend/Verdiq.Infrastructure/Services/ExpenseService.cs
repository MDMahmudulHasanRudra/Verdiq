using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Expense;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _context;

    public ExpenseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, ExpenseResponseDto? Data)> CreateAsync(CreateExpenseDto dto, Guid userId, Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found", null);

        var expense = new Expense
        {
            Description = dto.Description,
            Amount = dto.Amount,
            Category = dto.Category,
            ExpenseDate = dto.ExpenseDate,
            ChamberId = chamberId,
            CaseId = dto.CaseId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return (true, "Expense recorded successfully", MapToDto(expense));
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(Guid chamberId, string? category = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Expenses
            .Include(e => e.Case)
            .Include(e => e.User)
            .Where(e => e.ChamberId == chamberId && !e.IsDeleted);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return expenses.Select(MapToDto);
    }

    public async Task<decimal> GetTotalAsync(Guid chamberId)
    {
        return await _context.Expenses
            .Where(e => e.ChamberId == chamberId && !e.IsDeleted)
            .SumAsync(e => e.Amount);
    }

    private static ExpenseResponseDto MapToDto(Expense e)
    {
        return new ExpenseResponseDto
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            Currency = e.Currency,
            Category = e.Category,
            ExpenseDate = e.ExpenseDate,
            ReceiptPath = e.ReceiptPath,
            CaseId = e.CaseId,
            CaseTitle = e.Case != null ? e.Case.Title : null,
            CreatedByName = e.User != null ? e.User.FullName : "Unknown",
            CreatedAt = e.CreatedAt
        };
    }
}
