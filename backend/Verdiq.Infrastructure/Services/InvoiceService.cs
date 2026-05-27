using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Invoice;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, InvoiceResponseDto? Data)> CreateAsync(CreateInvoiceDto dto, Guid chamberId)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.ChamberId == chamberId && !c.IsDeleted);

        if (client == null)
            return (false, "Client not found", null);

        var invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            Amount = dto.Amount,
            Description = dto.Description,
            DueDate = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : null,
            Status = PaymentStatus.Pending,
            ClientId = dto.ClientId,
            CaseId = dto.CaseId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return (true, "Invoice created successfully", MapToDto(invoice));
    }

    public async Task<(bool Success, string Message)> MarkAsPaidAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null || invoice.IsDeleted)
            return (false, "Invoice not found");

        if (invoice.Status == PaymentStatus.Completed)
            return (false, "Invoice is already paid");

        invoice.Status = PaymentStatus.Completed;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, "Invoice marked as paid");
    }

    public async Task<IEnumerable<InvoiceResponseDto>> GetByClientIdAsync(Guid clientId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Case)
            .Where(i => i.ClientId == clientId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invoices.Select(MapToDto);
    }

    public async Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(Guid chamberId, string? status = null)
    {
        var query = _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Case)
            .Where(i => i.Client.ChamberId == chamberId && !i.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            query = query.Where(i => i.Status == paymentStatus);

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceResponseDto?> GetByIdAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Case)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        return invoice == null ? null : MapToDto(invoice);
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices
            .CountAsync(i => i.CreatedAt.Year == year) + 1;
        return $"INV-{year}-{count:D4}";
    }

    private static InvoiceResponseDto MapToDto(Invoice i)
    {
        return new InvoiceResponseDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Amount = i.Amount,
            Currency = i.Currency,
            Status = i.Status.ToString(),
            Description = i.Description,
            DueDate = i.DueDate,
            PaidAt = i.PaidAt,
            ClientId = i.ClientId,
            ClientName = i.Client.Name,
            CaseId = i.CaseId,
            CaseTitle = i.Case != null ? i.Case.Title : null,
            CreatedAt = i.CreatedAt
        };
    }
}
