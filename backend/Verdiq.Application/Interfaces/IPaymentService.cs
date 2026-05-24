using Verdiq.Application.DTOs.Payment;

namespace Verdiq.Application.Interfaces;

public interface IPaymentService
{
    Task<CheckoutResponseDto> InitiateCheckoutAsync(Guid userId, InitiateCheckoutDto dto);
    Task<PaymentResponseDto> GetPaymentAsync(Guid paymentId);
    Task<List<PaymentResponseDto>> GetUserPaymentsAsync(Guid userId);
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(Guid userId);
    Task<PaymentResponseDto> ProcessWebhookAsync(PaymentWebhookDto dto);
    Task<PaymentResponseDto> RefundPaymentAsync(Guid paymentId, Guid userId);
}
