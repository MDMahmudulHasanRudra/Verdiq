using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class TaxSetting : BaseEntity
{
    public TaxType TaxType { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal? Threshold { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
}

public class TaxTransaction : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid TaxSettingId { get; set; }
    public TaxSetting TaxSetting { get; set; } = null!;
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? ChallanNo { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Remarks { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
}
