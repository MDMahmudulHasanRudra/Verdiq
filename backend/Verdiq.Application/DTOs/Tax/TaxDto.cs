namespace Verdiq.Application.DTOs.Tax;

public class CreateTaxSettingDto
{
    public string TaxType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal? Threshold { get; set; }
    public string? Description { get; set; }
}

public class TaxSettingResponseDto
{
    public Guid Id { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal? Threshold { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaxTransactionDto
{
    public Guid TaxSettingId { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? ChallanNo { get; set; }
    public string? Remarks { get; set; }
}

public class TaxTransactionResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid TaxSettingId { get; set; }
    public string TaxTypeName { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? ChallanNo { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
