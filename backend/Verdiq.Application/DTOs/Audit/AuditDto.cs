namespace Verdiq.Application.DTOs.Audit;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid ChamberId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public List<AuditFieldChangeDto> Changes { get; set; } = new();
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ActionLabel => Action switch
    {
        "Added" => "Created",
        "Modified" => "Updated",
        "Deleted" => "Deleted",
        _ => Action
    };
}

public class AuditFieldChangeDto
{
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class AuditSummaryDto
{
    public int TotalActions { get; set; }
    public int TodayActions { get; set; }
    public Dictionary<string, int> ByEntity { get; set; } = new();
    public Dictionary<string, int> ByAction { get; set; } = new();
    public List<AuditLogResponseDto> RecentLogs { get; set; } = new();
}

public class AuditLogFilterDto
{
    public string? Entity { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
