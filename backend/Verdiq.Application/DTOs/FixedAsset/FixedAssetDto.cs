namespace Verdiq.Application.DTOs.FixedAsset;

public class CreateFixedAssetDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string DepreciationMethod { get; set; } = "StraightLine";
    public int UsefulLifeYears { get; set; }
    public decimal? SalvageValue { get; set; }
    public string? Location { get; set; }
    public string? Vendor { get; set; }
}

public class FixedAssetResponseDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public string DepreciationMethod { get; set; } = string.Empty;
    public int UsefulLifeYears { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public string? Location { get; set; }
    public string? Vendor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DisposalDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
