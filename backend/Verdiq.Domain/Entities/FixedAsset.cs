using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class FixedAsset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetDepreciationMethod DepreciationMethod { get; set; } = AssetDepreciationMethod.StraightLine;
    public int UsefulLifeYears { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public string? Location { get; set; }
    public string? Vendor { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public DateTime? DisposalDate { get; set; }
    public string? DisposalReason { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
}

public class AssetDepreciation : BaseEntity
{
    public Guid AssetId { get; set; }
    public FixedAsset Asset { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public int Period { get; set; }
    public string? Notes { get; set; }
}
