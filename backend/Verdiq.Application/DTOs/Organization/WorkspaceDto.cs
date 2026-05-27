namespace Verdiq.Application.DTOs.Organization;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
