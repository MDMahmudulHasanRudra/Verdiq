namespace Verdiq.Domain.Entities;

public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Role { get; set; } = "Member";
}
