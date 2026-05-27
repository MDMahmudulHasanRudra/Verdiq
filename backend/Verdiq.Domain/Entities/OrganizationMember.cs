using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class OrganizationMember : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;
    public string? InvitedEmail { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
