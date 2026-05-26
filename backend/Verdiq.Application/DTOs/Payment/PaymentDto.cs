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
    public string? TransactionId { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Description { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InitiateCheckoutDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Card";
    public string? PhoneNumber { get; set; }
    public Guid ClientId { get; set; }
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
