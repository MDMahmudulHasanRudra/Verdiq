using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Payment : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public GatewayName? Gateway { get; set; }
    public string? GatewayReference { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Description { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? ClientId { get; set; }

    public Subscription? Subscription { get; set; }
    public Client? Client { get; set; }
}
