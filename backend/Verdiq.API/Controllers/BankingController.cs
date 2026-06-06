using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Banking;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/banking")]
[Authorize]
public class BankingController : BaseController
{
    private readonly IBankingService _service;
    public BankingController(IBankingService service) => _service = service;

    [HttpGet("accounts")]
    public async Task<ActionResult<ApiResponse<List<BankAccountResponseDto>>>> GetAccounts()
        => Ok(ApiResponse<List<BankAccountResponseDto>>.Ok(await _service.GetAccountsAsync(GetChamberId())));

    [HttpGet("accounts/{id}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponseDto>>> GetAccount(Guid id)
    {
        var acc = await _service.GetAccountByIdAsync(id);
        return acc is null ? NotFound(ApiResponse<BankAccountResponseDto>.Fail("Not found"))
            : Ok(ApiResponse<BankAccountResponseDto>.Ok(acc));
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<ApiResponse<BankAccountResponseDto>>> CreateAccount([FromBody] CreateBankAccountDto dto)
        => Ok(ApiResponse<BankAccountResponseDto>.Ok(await _service.CreateAccountAsync(dto, GetChamberId())));

    [HttpPut("accounts/{id}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponseDto>>> UpdateAccount(Guid id, [FromBody] CreateBankAccountDto dto)
        => Ok(ApiResponse<BankAccountResponseDto>.Ok(await _service.UpdateAccountAsync(id, dto)));

    [HttpDelete("accounts/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAccount(Guid id)
    {
        await _service.DeleteAccountAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Account deleted"));
    }

    [HttpGet("accounts/{id}/transactions")]
    public async Task<ActionResult<ApiResponse<List<BankTransactionResponseDto>>>> GetTransactions(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(ApiResponse<List<BankTransactionResponseDto>>.Ok(await _service.GetTransactionsAsync(id, page, pageSize)));

    [HttpPost("transactions")]
    public async Task<ActionResult<ApiResponse<BankTransactionResponseDto>>> CreateTransaction([FromBody] CreateBankTransactionDto dto)
        => Ok(ApiResponse<BankTransactionResponseDto>.Ok(await _service.CreateTransactionAsync(dto)));

    [HttpPost("transactions/{id}/reconcile")]
    public async Task<ActionResult<ApiResponse<BankTransactionResponseDto>>> ReconcileTransaction(Guid id)
        => Ok(ApiResponse<BankTransactionResponseDto>.Ok(await _service.ReconcileTransactionAsync(id)));

    [HttpPost("accounts/{id}/reconcile")]
    public async Task<ActionResult<ApiResponse<BankAccountResponseDto>>> ReconcileAccount(Guid id)
        => Ok(ApiResponse<BankAccountResponseDto>.Ok(await _service.ReconcileAccountAsync(id)));
}
