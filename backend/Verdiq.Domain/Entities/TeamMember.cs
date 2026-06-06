namespace Verdiq.Domain.Entities;

public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string? Email { get; set; }
    public string? InvitedName { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime? AcceptedAt { get; set; }
}
