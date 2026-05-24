using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Payment;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<CheckoutResponseDto>>> InitiateCheckout([FromBody] InitiateCheckoutDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _paymentService.InitiateCheckoutAsync(userId, dto);
            return Ok(ApiResponse<CheckoutResponseDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CheckoutResponseDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CheckoutResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Webhook([FromBody] PaymentWebhookDto dto)
    {
        try
        {
            var result = await _paymentService.ProcessWebhookAsync(dto);
            return Ok(ApiResponse<PaymentResponseDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetById(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentAsync(id);
            return Ok(ApiResponse<PaymentResponseDto>.Ok(payment));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PaymentResponseDto>>>> GetMyPayments()
    {
        var userId = GetUserId();
        var payments = await _paymentService.GetUserPaymentsAsync(userId);
        return Ok(ApiResponse<List<PaymentResponseDto>>.Ok(payments));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PaymentHistoryDto>>> GetPaymentHistory()
    {
        var userId = GetUserId();
        var history = await _paymentService.GetPaymentHistoryAsync(userId);
        return Ok(ApiResponse<PaymentHistoryDto>.Ok(history));
    }

    [HttpPost("{id}/refund")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Refund(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _paymentService.RefundPaymentAsync(id, userId);
            return Ok(ApiResponse<PaymentResponseDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentResponseDto>.Fail(ex.Message));
        }
    }
}
