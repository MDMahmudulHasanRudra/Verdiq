using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Chamber : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}
