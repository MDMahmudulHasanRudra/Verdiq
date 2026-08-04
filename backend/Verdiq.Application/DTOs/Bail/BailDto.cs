namespace Verdiq.Application.DTOs.Bail;

public class CreateBailDto
{
    public Guid CaseId { get; set; }
    public string BailType { get; set; } = "Regular";
    public decimal? BailAmount { get; set; }
    public string? BailConditions { get; set; }
    public DateTime? BailHearingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? BondNumber { get; set; }
    public string? SuretyName { get; set; }
    public string? SuretyAddress { get; set; }
    public string? SuretyContact { get; set; }
    public string? GrantedBy { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBailDto
{
    public string? BailType { get; set; }
    public decimal? BailAmount { get; set; }
    public string? BailConditions { get; set; }
    public DateTime? BailHearingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? BondNumber { get; set; }
    public string? SuretyName { get; set; }
    public string? SuretyAddress { get; set; }
    public string? SuretyContact { get; set; }
    public string? GrantedBy { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBailStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? RevokedReason { get; set; }
}

public class BailResponseDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BailType { get; set; } = string.Empty;
    public decimal? BailAmount { get; set; }
    public string? BailConditions { get; set; }
    public DateTime? BailGrantedAt { get; set; }
    public DateTime? BailHearingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? BondNumber { get; set; }
    public string? SuretyName { get; set; }
    public string? SuretyAddress { get; set; }
    public string? SuretyContact { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? GrantedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
