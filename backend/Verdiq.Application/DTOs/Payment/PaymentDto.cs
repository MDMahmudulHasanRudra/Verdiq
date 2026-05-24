namespace Verdiq.Application.DTOs.Payment;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Gateway { get; set; }
    public string? GatewayReference { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Description { get; set; }
    public Guid? SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string PaymentMethod { get; set; } = "Card";
    public string? Gateway { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public Guid? SubscriptionId { get; set; }
}

public class InitiateCheckoutDto
{
    public string Plan { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Card";
    public string? PhoneNumber { get; set; }
}

public class CheckoutResponseDto
{
    public PaymentResponseDto Payment { get; set; } = null!;
    public string? CheckoutUrl { get; set; }
    public string? ClientSecret { get; set; }
}

public class PaymentWebhookDto
{
    public string Gateway { get; set; } = string.Empty;
    public string GatewayReference { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string? PhoneNumber { get; set; }
}

public class PaymentHistoryDto
{
    public List<PaymentResponseDto> Payments { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
