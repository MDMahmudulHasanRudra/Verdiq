using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
