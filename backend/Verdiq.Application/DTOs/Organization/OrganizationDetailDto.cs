namespace Verdiq.Application.DTOs.Organization;

public class OrganizationDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int WorkspaceCount { get; set; }
    public List<OrganizationMemberDto> Members { get; set; } = new();
    public List<WorkspaceDto> Workspaces { get; set; } = new();
}
