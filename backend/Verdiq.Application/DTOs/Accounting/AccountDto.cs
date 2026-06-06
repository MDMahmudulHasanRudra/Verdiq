using Verdiq.Domain.Enums;

namespace Verdiq.Application.DTOs.Accounting;

public class CreateAccountDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public decimal OpeningBalance { get; set; }
}

public class UpdateAccountDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public AccountType? Type { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? OpeningBalance { get; set; }
}

public class AccountResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public bool IsActive { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Balance { get; set; }
    public List<AccountResponseDto> Children { get; set; } = new();
}
