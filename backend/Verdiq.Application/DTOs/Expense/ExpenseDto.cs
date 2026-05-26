namespace Verdiq.Application.DTOs.Expense;

public class CreateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public Guid? CaseId { get; set; }
}

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptPath { get; set; }
    public Guid? CaseId { get; set; }
    public string? CaseTitle { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
