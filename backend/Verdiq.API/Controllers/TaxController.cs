using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Tax;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/tax")]
[Authorize]
public class TaxController : BaseController
{
    private readonly ITaxService _service;
    public TaxController(ITaxService service) => _service = service;

    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<List<TaxSettingResponseDto>>>> GetSettings()
        => Ok(ApiResponse<List<TaxSettingResponseDto>>.Ok(await _service.GetTaxSettingsAsync(GetChamberId())));

    [HttpPost("settings")]
    public async Task<ActionResult<ApiResponse<TaxSettingResponseDto>>> CreateSetting([FromBody] CreateTaxSettingDto dto)
        => Ok(ApiResponse<TaxSettingResponseDto>.Ok(await _service.CreateTaxSettingAsync(dto, GetChamberId())));

    [HttpPut("settings/{id}")]
    public async Task<ActionResult<ApiResponse<TaxSettingResponseDto>>> UpdateSetting(Guid id, [FromBody] CreateTaxSettingDto dto)
        => Ok(ApiResponse<TaxSettingResponseDto>.Ok(await _service.UpdateTaxSettingAsync(id, dto)));

    [HttpDelete("settings/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSetting(Guid id)
    {
        await _service.DeleteTaxSettingAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Setting deleted"));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<List<TaxTransactionResponseDto>>>> GetTransactions([FromQuery] int? year)
        => Ok(ApiResponse<List<TaxTransactionResponseDto>>.Ok(await _service.GetTaxTransactionsAsync(GetChamberId(), year)));

    [HttpPost("transactions")]
    public async Task<ActionResult<ApiResponse<TaxTransactionResponseDto>>> CreateTransaction([FromBody] CreateTaxTransactionDto dto)
        => Ok(ApiResponse<TaxTransactionResponseDto>.Ok(await _service.CreateTaxTransactionAsync(dto, GetChamberId())));

    [HttpGet("liability")]
    public async Task<ActionResult<ApiResponse<object>>> GetLiability([FromQuery] int year)
    {
        var total = await _service.GetTotalTaxLiabilityAsync(GetChamberId(), year);
        return Ok(new { success = true, data = new { totalLiability = total, year } });
    }
}
