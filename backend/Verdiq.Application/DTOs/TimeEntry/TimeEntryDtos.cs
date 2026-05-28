namespace Verdiq.Application.DTOs.TimeEntry;

public class CreateTimeEntryDto
{
    public Guid? ClientId { get; set; }
    public Guid? CaseId { get; set; }
    public Guid? TaskId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public bool Billable { get; set; } = true;
    public string Status { get; set; } = "Running";
}

public class UpdateTimeEntryDto
{
    public Guid? ClientId { get; set; }
    public Guid? CaseId { get; set; }
    public Guid? TaskId { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? DurationMinutes { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool? Billable { get; set; }
    public string? Status { get; set; }
}

public class UpdateTimeEntryStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class TimeEntryResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? CaseId { get; set; }
    public string? CaseTitle { get; set; }
    public string? CaseNumber { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool Billable { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TimeSheetAnalyticsDto
{
    public double TotalBillableHours { get; set; }
    public double TotalNonBillableHours { get; set; }
    public double UtilizationPercent { get; set; }
    public decimal RevenueEstimate { get; set; }
    public double TotalDurationMinutes { get; set; }
    public int TotalEntries { get; set; }
    public int BillableEntries { get; set; }
    public int NonBillableEntries { get; set; }
    public List<TimeEntryByLawyer> ByLawyer { get; set; } = new();
    public List<TimeEntryByClient> ByClient { get; set; } = new();
    public List<TimeEntryByCase> ByCase { get; set; } = new();
    public List<TimeEntryByDay> ByDay { get; set; } = new();
    public List<TimeEntryByCategory> ByCategory { get; set; } = new();
    public List<MonthlyRevenueTrend> MonthlyTrend { get; set; } = new();
}

public class TimeEntryByLawyer
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public double BillableHours { get; set; }
    public double UtilizationPercent { get; set; }
    public decimal Revenue { get; set; }
}

public class TimeEntryByClient
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public decimal Revenue { get; set; }
}

public class TimeEntryByCase
{
    public Guid CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public decimal Revenue { get; set; }
}

public class TimeEntryByDay
{
    public string Date { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public double BillableHours { get; set; }
    public decimal Revenue { get; set; }
}

public class TimeEntryByCategory
{
    public string Category { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public int Count { get; set; }
}

public class MonthlyRevenueTrend
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public double BillableHours { get; set; }
}

public class TeamCapacityDto
{
    public int TotalLawyers { get; set; }
    public double TotalAvailableHours { get; set; }
    public double TotalBookedHours { get; set; }
    public double UtilizationPercent { get; set; }
    public int InactiveLawyers { get; set; }
    public List<LawyerUtilization> Lawyers { get; set; } = new();
}

public class LawyerUtilization
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double AvailableHours { get; set; }
    public double BookedHours { get; set; }
    public double UtilizationPercent { get; set; }
    public bool IsInactive { get; set; }
    public decimal Revenue { get; set; }
}
