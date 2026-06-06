namespace Verdiq.Domain.Entities;

public class TaskWatcher : BaseEntity
{
    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
