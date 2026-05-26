using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Status { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BarCouncilId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? TwoFactorVerifiedAt { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<Case> AssignedCases { get; set; } = new List<Case>();
    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public Subscription? Subscription { get; set; }
}
