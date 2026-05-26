using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Invoice;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : BaseController
{
    private readonly IInvoiceService _invoiceService;

    public PaymentsController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InvoiceResponseDto>>>> GetAll(
        [FromQuery] string? status = null)
    {
        var chamberId = GetChamberId();
        var invoices = await _invoiceService.GetAllAsync(chamberId, status);
        return Ok(ApiResponse<List<InvoiceResponseDto>>.Ok(invoices.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetById(Guid id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
            return NotFound(ApiResponse<InvoiceResponseDto>.Fail("Invoice not found"));

        return Ok(ApiResponse<InvoiceResponseDto>.Ok(invoice));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Create(
        [FromBody] CreateInvoiceDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _invoiceService.CreateAsync(dto, chamberId);

        if (!success)
            return BadRequest(ApiResponse<InvoiceResponseDto>.Fail(message));

        return Ok(ApiResponse<InvoiceResponseDto>.Created(data!));
    }

    [HttpPost("{id}/pay")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsPaid(Guid id)
    {
        var (success, message) = await _invoiceService.MarkAsPaidAsync(id);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("by-client/{clientId}")]
    public async Task<ActionResult<ApiResponse<List<InvoiceResponseDto>>>> GetByClient(Guid clientId)
    {
        var invoices = await _invoiceService.GetByClientIdAsync(clientId);
        return Ok(ApiResponse<List<InvoiceResponseDto>>.Ok(invoices.ToList()));
    }
}
