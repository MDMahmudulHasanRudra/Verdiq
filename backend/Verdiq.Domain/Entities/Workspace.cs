namespace Verdiq.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}
