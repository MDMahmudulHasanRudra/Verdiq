namespace Verdiq.Application.DTOs.Audit;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditSummaryDto
{
    public int TotalActions { get; set; }
    public int TodayActions { get; set; }
    public Dictionary<string, int> ByEntity { get; set; } = new();
    public Dictionary<string, int> ByAction { get; set; } = new();
    public List<AuditLogResponseDto> RecentLogs { get; set; } = new();
}
