namespace Verdiq.Application.DTOs.Subscription;

public class SubscriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid ChamberId { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
}

public class ChangePlanDto
{
    public string Plan { get; set; } = string.Empty;
}
