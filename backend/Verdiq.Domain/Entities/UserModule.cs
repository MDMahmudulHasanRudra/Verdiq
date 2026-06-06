namespace Verdiq.Domain.Entities;

public class UserModule : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string ModuleName { get; set; } = string.Empty;
}
