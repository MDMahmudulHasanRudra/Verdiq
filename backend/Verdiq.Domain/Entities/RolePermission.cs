using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class RolePermission : BaseEntity
{
    public UserRole Role { get; set; }
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
