namespace Verdiq.Application.DTOs.Team;

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid>? MemberIds { get; set; }
}

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class TeamResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public List<TeamMemberDto> Members { get; set; } = new();
}

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string TeamRole { get; set; } = "Member";
    public DateTime JoinedAt { get; set; }
    public bool IsPending { get; set; }
    public string? InvitedName { get; set; }
}

public class AddTeamMemberDto
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? InvitedName { get; set; }
    public string? Password { get; set; }
    public string? UserRole { get; set; }
    public string Role { get; set; } = "Member";
}

public class UpdateTeamMemberRoleDto
{
    public string Role { get; set; } = "Member";
}
