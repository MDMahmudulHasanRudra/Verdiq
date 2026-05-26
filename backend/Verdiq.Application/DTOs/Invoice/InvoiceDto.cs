namespace Verdiq.Application.DTOs.Invoice;

public class CreateInvoiceDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid ClientId { get; set; }
    public Guid? CaseId { get; set; }
}

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? CaseId { get; set; }
    public string? CaseTitle { get; set; }
    public DateTime CreatedAt { get; set; }
}
