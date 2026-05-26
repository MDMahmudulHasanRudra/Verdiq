namespace Verdiq.Application.DTOs.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? BarCouncilId { get; set; }
    public Guid ChamberId { get; set; }
    public string ChamberName { get; set; } = string.Empty;
    public int CasesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminCaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string AssignedLawyerName { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminRevenueDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Transactions { get; set; }
}

public class AdminSystemStatsDto
{
    public int TotalChambers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCases { get; set; }
    public int TotalClients { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
