namespace Verdiq.Application.DTOs.Client;

public class CreateClientDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateClientDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}

public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int CasesCount { get; set; }
    public DateTime JoinedDate { get; set; }
    public decimal TotalPayments { get; set; }
}
