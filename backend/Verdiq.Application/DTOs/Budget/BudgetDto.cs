namespace Verdiq.Application.DTOs.Budget;

public class CreateBudgetDto
{
    public string Name { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public List<BudgetLineDto> Lines { get; set; } = new();
}

public class BudgetLineDto
{
    public Guid AccountId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class BudgetResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<BudgetLineResponseDto> Lines { get; set; } = new();
}

public class BudgetLineResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal Remaining { get; set; }
    public decimal UsagePercent { get; set; }
}
