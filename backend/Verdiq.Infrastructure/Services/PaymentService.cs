using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Payment;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutResponseDto> InitiateCheckoutAsync(Guid userId, InitiateCheckoutDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found. Create a subscription first.");

        if (!Enum.TryParse<SubscriptionPlan>(dto.Plan, true, out var plan))
            throw new ArgumentException("Invalid plan. Valid: Free, Pro, Chamber");

        var planPrices = new Dictionary<SubscriptionPlan, decimal>
        {
            [SubscriptionPlan.Pro] = 29.99m,
            [SubscriptionPlan.Chamber] = 99.99m
        };

        if (!planPrices.TryGetValue(plan, out var amount))
            throw new ArgumentException("Free plan does not require payment");

        if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
            throw new ArgumentException("Invalid payment method. Valid: Card, bkash, Nagad, BankTransfer");

        var gateway = paymentMethod switch
        {
            PaymentMethod.Card => GatewayName.Stripe,
            PaymentMethod.bkash => GatewayName.Bkash,
            PaymentMethod.Nagad => GatewayName.Nagad,
            _ => throw new ArgumentException("Unsupported payment method")
        };

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var payment = new Payment
        {
            InvoiceNumber = invoiceNumber,
            Amount = amount,
            Currency = "BDT",
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            Gateway = gateway,
            PhoneNumber = dto.PhoneNumber,
            Description = $"Subscription: {plan} Plan",
            SubscriptionId = subscription.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var checkoutUrl = GenerateCheckoutUrl(gateway, payment, user);
        if (!string.IsNullOrEmpty(checkoutUrl))
        {
            payment.PaymentUrl = checkoutUrl;
            await _context.SaveChangesAsync();
        }

        return new CheckoutResponseDto
        {
            Payment = MapToDto(payment),
            CheckoutUrl = checkoutUrl,
            ClientSecret = gateway == GatewayName.Stripe ? $"pi_{payment.Id:N}_secret_{Guid.NewGuid():N}" : null
        };
    }

    public async Task<PaymentResponseDto> GetPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId)
            ?? throw new KeyNotFoundException("Payment not found");
        return MapToDto(payment);
    }

    public async Task<List<PaymentResponseDto>> GetUserPaymentsAsync(Guid userId)
    {
        var subscriptionIds = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => subscriptionIds.Contains(p.SubscriptionId ?? Guid.Empty) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentHistoryDto> GetPaymentHistoryAsync(Guid userId)
    {
        var subscriptionIds = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => subscriptionIds.Contains(p.SubscriptionId ?? Guid.Empty) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return new PaymentHistoryDto
        {
            Payments = payments.Select(MapToDto).ToList(),
            TotalCount = payments.Count,
            TotalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount)
        };
    }

    public async Task<PaymentResponseDto> ProcessWebhookAsync(PaymentWebhookDto dto)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.GatewayReference == dto.GatewayReference);

        if (payment == null)
        {
            if (Guid.TryParse(dto.GatewayReference, out var guid))
                payment = await _context.Payments.FindAsync(guid);
        }

        if (payment == null)
            throw new KeyNotFoundException("Payment not found for this reference");

        payment.TransactionId = dto.TransactionId;
        payment.GatewayReference = dto.GatewayReference;
        payment.PhoneNumber = dto.PhoneNumber ?? payment.PhoneNumber;

        if (dto.Status.Equals("completed", StringComparison.OrdinalIgnoreCase) ||
            dto.Status.Equals("success", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            await UpdateSubscriptionAfterPayment(payment);
        }
        else if (dto.Status.Equals("failed", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = dto.FailureReason;
        }
        else if (dto.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Cancelled;
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    public async Task<PaymentResponseDto> RefundPaymentAsync(Guid paymentId, Guid userId)
    {
        var payment = await _context.Payments.FindAsync(paymentId)
            ?? throw new KeyNotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    private async Task UpdateSubscriptionAfterPayment(Payment payment)
    {
        if (payment.SubscriptionId == null) return;

        var subscription = await _context.Subscriptions.FindAsync(payment.SubscriptionId.Value);
        if (subscription == null) return;

        var planFromAmount = payment.Amount switch
        {
            29.99m => SubscriptionPlan.Pro,
            99.99m => SubscriptionPlan.Chamber,
            _ => (SubscriptionPlan?)null
        };

        if (planFromAmount.HasValue)
        {
            subscription.Plan = planFromAmount.Value;
        }

        subscription.Status = SubscriptionStatus.Active;
        subscription.CurrentPeriodStart = DateTime.UtcNow;
        subscription.CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1);
        subscription.CancelAtPeriodEnd = false;
        subscription.UpdatedAt = DateTime.UtcNow;
    }

    private static string? GenerateCheckoutUrl(GatewayName gateway, Payment payment, User user)
    {
        return gateway switch
        {
            GatewayName.Stripe => $"https://checkout.stripe.com/pay/{payment.Id}",
            GatewayName.Bkash => $"https://checkout.bkash.com/pay?invoice={payment.InvoiceNumber}&amount={payment.Amount}",
            GatewayName.Nagad => $"https://checkout.nagad.com/pay?invoice={payment.InvoiceNumber}&amount={payment.Amount}",
            _ => null
        };
    }

    private static PaymentResponseDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        InvoiceNumber = p.InvoiceNumber,
        Amount = p.Amount,
        Currency = p.Currency,
        PaymentMethod = p.PaymentMethod.ToString(),
        Status = p.Status.ToString(),
        Gateway = p.Gateway?.ToString(),
        GatewayReference = p.GatewayReference,
        PaymentUrl = p.PaymentUrl,
        TransactionId = p.TransactionId,
        PhoneNumber = p.PhoneNumber,
        FailureReason = p.FailureReason,
        PaidAt = p.PaidAt,
        Description = p.Description,
        SubscriptionId = p.SubscriptionId,
        CreatedAt = p.CreatedAt
    };
}
