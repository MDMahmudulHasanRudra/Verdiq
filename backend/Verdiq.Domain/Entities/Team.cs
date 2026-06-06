namespace Verdiq.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
