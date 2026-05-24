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
    public string? AvatarUrl { get; set; }
    public int CasesCount { get; set; }
    public string? SubscriptionPlan { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminCaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
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
    public int TotalUsers { get; set; }
    public int ActiveLawyers { get; set; }
    public int TotalClients { get; set; }
    public int TotalCases { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public long StorageUsed { get; set; }
    public DatabaseStatsDto Database { get; set; } = new();
}

public class DatabaseStatsDto
{
    public int ActiveConnections { get; set; }
    public string Size { get; set; } = string.Empty;
    public string LastBackup { get; set; } = string.Empty;
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
