namespace Verdiq.Application.DTOs.Permission;

public class PermissionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class RolePermissionResponseDto
{
    public string Role { get; set; } = string.Empty;
    public List<PermissionResponseDto> Permissions { get; set; } = new();
}
