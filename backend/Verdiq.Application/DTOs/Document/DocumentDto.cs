namespace Verdiq.Application.DTOs.Document;

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string StorageProvider { get; set; } = "Local";
    public int CurrentVersion { get; set; } = 1;
    public int VersionCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<DocumentVersionDto> Versions { get; set; } = new();
}

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChangeNotes { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DocumentTagDto
{
    public Guid Id { get; set; }
    public string TagName { get; set; } = string.Empty;
}

public class BulkOperationResult
{
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class CaseStatsDto
{
    public int TotalCases { get; set; }
    public int ActiveCases { get; set; }
    public int PendingCases { get; set; }
    public int ClosedCases { get; set; }
    public int HearingsToday { get; set; }
    public int UpcomingHearings { get; set; }
    public int TotalClients { get; set; }
    public int UnreadNotifications { get; set; }
}

public class DashboardResponseDto
{
    public CaseStatsDto Stats { get; set; } = new();
    public List<RecentCaseDto> RecentCases { get; set; } = new();
    public List<UpcomingHearingDto> UpcomingHearings { get; set; } = new();
}

public class RecentCaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpcomingHearingDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public DateTime HearingDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
