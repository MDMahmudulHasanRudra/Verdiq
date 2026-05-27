namespace Verdiq.Application.DTOs.Organization;

public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
