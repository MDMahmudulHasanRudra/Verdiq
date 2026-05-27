using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
