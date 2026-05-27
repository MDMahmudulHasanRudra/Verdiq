using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Payment;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

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

        if (!Enum.TryParse<Domain.Enums.PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
            throw new ArgumentException("Invalid payment method. Valid: Card, bkash, Nagad, BankTransfer");

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var payment = new Payment
        {
            InvoiceNumber = invoiceNumber,
            Amount = dto.Amount,
            Currency = "BDT",
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            ClientId = dto.ClientId,
            PhoneNumber = dto.PhoneNumber,
            Description = "Payment",
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new CheckoutResponseDto
        {
            Payment = MapToDto(payment),
            CheckoutUrl = null,
            ClientSecret = null
        };
    }

    public async Task<PaymentResponseDto> GetPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found");
        return MapToDto(payment);
    }

    public async Task<List<PaymentResponseDto>> GetUserPaymentsAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new();

        var clientIds = await _context.Clients
            .Where(c => c.ChamberId == user.ChamberId && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        var payments = await _context.Payments
            .Include(p => p.Client)
            .Where(p => clientIds.Contains(p.ClientId) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentHistoryDto> GetPaymentHistoryAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new PaymentHistoryDto();

        var clientIds = await _context.Clients
            .Where(c => c.ChamberId == user.ChamberId && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        var payments = await _context.Payments
            .Include(p => p.Client)
            .Where(p => clientIds.Contains(p.ClientId) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return new PaymentHistoryDto
        {
            Payments = payments.Select(MapToDto).ToList(),
            TotalPaid = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
            TotalPending = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
            TotalRefunded = payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.Amount),
            TotalTransactions = payments.Count
        };
    }

    public async Task<PaymentResponseDto> ProcessWebhookAsync(PaymentWebhookDto dto)
    {
        var payment = await _context.Payments
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.TransactionId == dto.TransactionId);

        if (payment == null)
            throw new KeyNotFoundException("Payment not found for this reference");

        payment.TransactionId = dto.TransactionId;
        payment.PhoneNumber = dto.PhoneNumber ?? payment.PhoneNumber;

        if (dto.Status.Equals("completed", StringComparison.OrdinalIgnoreCase) ||
            dto.Status.Equals("success", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
        }
        else if (dto.Status.Equals("failed", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Failed;
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
        var payment = await _context.Payments
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    private static PaymentResponseDto MapToDto(Payment p)
    {
        return new PaymentResponseDto
        {
            Id = p.Id,
            InvoiceNumber = p.InvoiceNumber,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod.ToString(),
            Status = p.Status.ToString(),
            Gateway = p.Gateway?.ToString(),
            TransactionId = p.TransactionId,
            PhoneNumber = p.PhoneNumber,
            PaidAt = p.PaidAt,
            Description = p.Description,
            ClientId = p.ClientId,
            ClientName = p.Client?.Name ?? "Unknown",
            CreatedAt = p.CreatedAt
        };
    }
}
