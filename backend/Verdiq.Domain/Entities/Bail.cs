using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Bail : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public BailStatus Status { get; set; } = BailStatus.Pending;
    public string BailType { get; set; } = "Regular";
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
}
