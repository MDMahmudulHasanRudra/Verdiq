using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Invoice;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : BaseController
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Create([FromBody] CreateInvoiceDto dto)
    {
        var chamberId = GetUserId();
        var (success, message, data) = await _invoiceService.CreateAsync(dto, chamberId);

        if (!success)
            return BadRequest(ApiResponse<InvoiceResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<InvoiceResponseDto>.Created(data));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetAll([FromQuery] string? status)
    {
        var chamberId = GetUserId();
        var invoices = await _invoiceService.GetAllAsync(chamberId, status);
        return Ok(ApiResponse<IEnumerable<InvoiceResponseDto>>.Ok(invoices));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetById(Guid id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);

        if (invoice is null)
            return NotFound(ApiResponse<InvoiceResponseDto>.Fail("Invoice not found"));

        return Ok(ApiResponse<InvoiceResponseDto>.Ok(invoice));
    }

    [HttpGet("by-client/{clientId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetByClient(Guid clientId)
    {
        var invoices = await _invoiceService.GetByClientIdAsync(clientId);
        return Ok(ApiResponse<IEnumerable<InvoiceResponseDto>>.Ok(invoices));
    }

    [HttpPost("{id}/mark-paid")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsPaid(Guid id)
    {
        var (success, message) = await _invoiceService.MarkAsPaidAsync(id);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
