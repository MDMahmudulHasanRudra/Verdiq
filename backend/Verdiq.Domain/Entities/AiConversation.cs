namespace Verdiq.Domain.Entities;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }

    public User User { get; set; } = null!;
}
