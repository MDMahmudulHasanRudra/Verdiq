namespace Verdiq.Domain.Entities;

public class TimeEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? CaseId { get; set; }
    public Case? Case { get; set; }
    public Guid? TaskId { get; set; }
    public Task? Task { get; set; }
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public bool Billable { get; set; } = true;

    /// <summary>Running, Paused, Completed, Invoiced</summary>
    public string Status { get; set; } = "Running";

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
}
