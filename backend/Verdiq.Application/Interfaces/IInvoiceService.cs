using Verdiq.Application.DTOs.Invoice;

namespace Verdiq.Application.Interfaces;

public interface IInvoiceService
{
    Task<(bool Success, string Message, InvoiceResponseDto? Data)> CreateAsync(CreateInvoiceDto dto, Guid chamberId);
    Task<(bool Success, string Message)> MarkAsPaidAsync(Guid id);
    Task<IEnumerable<InvoiceResponseDto>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(Guid chamberId, string? status = null);
    Task<InvoiceResponseDto?> GetByIdAsync(Guid id);
}
