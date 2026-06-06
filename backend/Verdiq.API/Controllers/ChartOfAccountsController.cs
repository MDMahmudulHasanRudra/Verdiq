using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Accounting;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/accounting/charts")]
[Authorize]
public class ChartOfAccountsController : BaseController
{
    private readonly IChartOfAccountService _service;

    public ChartOfAccountsController(IChartOfAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AccountResponseDto>>>> GetAll([FromQuery] bool tree = false)
    {
        var chamberId = GetChamberId();
        var accounts = tree
            ? await _service.GetTreeAsync(chamberId)
            : await _service.GetAllAsync(chamberId);
        return Ok(ApiResponse<List<AccountResponseDto>>.Ok(accounts));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountResponseDto>>> GetById(Guid id)
    {
        var account = await _service.GetByIdAsync(id);
        if (account is null)
            return NotFound(ApiResponse<AccountResponseDto>.Fail("Account not found"));
        return Ok(ApiResponse<AccountResponseDto>.Ok(account));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountResponseDto>>> Create([FromBody] CreateAccountDto dto)
    {
        try
        {
            var chamberId = GetChamberId();
            var account = await _service.CreateAsync(dto, chamberId);
            return CreatedAtAction(nameof(GetById), new { id = account.Id },
                ApiResponse<AccountResponseDto>.Created(account));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AccountResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AccountResponseDto>>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        try
        {
            var account = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<AccountResponseDto>.Ok(account));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AccountResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Account deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
