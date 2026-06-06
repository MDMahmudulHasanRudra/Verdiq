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
    public int DocumentsCount { get; set; }
    public int HearingsCount { get; set; }
    public int InvoicesCount { get; set; }
}

public class SubscriptionManageDto
{
    public Guid Id { get; set; }
    public Guid ChamberId { get; set; }
    public string ChamberName { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public string? UserFullName { get; set; }
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
    public string? SubscriptionPlan { get; set; }
    public string? SubscriptionStatus { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
}

public class UpdateUserSubscriptionDto
{
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CurrentPeriodEnd { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ImpersonateDto
{
    public Guid? UserId { get; set; }
}

public class CreateAdminUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Owner";
    public Guid ChamberId { get; set; }
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
    public decimal TotalRevenueAllTime { get; set; }
    public int NewChambersThisMonth { get; set; }
    public int NewCasesThisMonth { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalHearings { get; set; }
    public int TotalPayments { get; set; }
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
    public int HearingsDeleted { get; set; }
    public int ExpensesDeleted { get; set; }
    public int TasksDeleted { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class RolePermissionAssignmentDto
{
    public string Role { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
}

public class RolePermissionsDto
{
    public string Role { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Entity { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BillingOverviewDto
{
    public int TotalInvoices { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PendingPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal PendingAmount { get; set; }
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();
}

public class RecentPaymentDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? ChamberName { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreateChamberDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string Plan { get; set; } = "Free";
}

public class UpdateChamberDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class BroadcastNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "system";
    public Guid? TargetChamberId { get; set; }
}

public class SystemConfigDto
{
    public bool AllowSelfRegistration { get; set; } = true;
    public bool MaintenanceMode { get; set; } = false;
    public int TrialDays { get; set; } = 14;
    public int MaxLoginAttempts { get; set; } = 5;
    public bool RequireEmailVerification { get; set; } = false;
    public bool EnableAiFeatures { get; set; } = true;
    public string DefaultCurrency { get; set; } = "BDT";
}

public class ChamberPermissionDto
{
    public Guid ChamberId { get; set; }
    public string ChamberName { get; set; } = string.Empty;
    public List<RolePermissionsDto> RolePermissions { get; set; } = new();
}
