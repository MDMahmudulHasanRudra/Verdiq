namespace Verdiq.Application.DTOs.SuperAdmin;

public class SuperAdminLoginDto
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SuperAdminAuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public SuperAdminInfo? Admin { get; set; }
}

public class SuperAdminInfo
{
    public string Id { get; set; } = "superadmin";
    public string Name { get; set; } = "Super Admin";
    public string UserId { get; set; } = "rudra";
    public string Role { get; set; } = "SuperAdmin";
}

public class ChamberManageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public int CasesCount { get; set; }
    public int ClientsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateChamberPlanDto
{
    public string Plan { get; set; } = string.Empty;
}

public class SuperAdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid ChamberId { get; set; }
    public string ChamberName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ImpersonateDto
{
    public Guid ChamberId { get; set; }
    public Guid? UserId { get; set; }
}

public class SystemHealthDto
{
    public string Status { get; set; } = "Healthy";
    public string DatabaseStatus { get; set; } = string.Empty;
    public long DatabaseSizeBytes { get; set; }
    public int ActiveConnections { get; set; }
    public int TotalChambers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCases { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public long StorageUsedBytes { get; set; }
    public string Uptime { get; set; } = string.Empty;
    public string LastBackup { get; set; } = string.Empty;
    public List<string> ActiveAlerts { get; set; } = new();
}

public class SuperAdminDashboardDto
{
    public int TotalChambers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCases { get; set; }
    public int TotalClients { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int NewChambersThisMonth { get; set; }
    public int NewCasesThisMonth { get; set; }
    public List<ChamberManageDto> Chambers { get; set; } = new();
    public List<SystemAlert> Alerts { get; set; } = new();
}

public class SystemAlert
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public DateTime Timestamp { get; set; }
}

public class ClearChamberResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UsersDeleted { get; set; }
    public int CasesDeleted { get; set; }
    public int ClientsDeleted { get; set; }
    public int DocumentsDeleted { get; set; }
    public int InvoicesDeleted { get; set; }
}
