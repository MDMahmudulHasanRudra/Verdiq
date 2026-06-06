using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Task : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public string? Priority { get; set; }

    public Guid AssignedTo { get; set; }
    public User AssignedUser { get; set; } = null!;
    public Guid AssignedBy { get; set; }
    public User Assigner { get; set; } = null!;
    public Guid? CaseId { get; set; }
    public Case? Case { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;

    public int SortOrder { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<TaskWatcher> Watchers { get; set; } = new List<TaskWatcher>();
}
