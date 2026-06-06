using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Accounting;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/accounting")]
[Authorize]
public class AccountingController : BaseController
{
    private readonly IAccountingService _service;

    public AccountingController(IAccountingService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AccountingDashboardDto>>> GetDashboard()
    {
        var chamberId = GetChamberId();
        var dashboard = await _service.GetDashboardAsync(chamberId);
        return Ok(ApiResponse<AccountingDashboardDto>.Ok(dashboard));
    }

    [HttpGet("profit-loss")]
    public async Task<ActionResult<ApiResponse<ProfitLossDto>>> GetProfitLoss(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var chamberId = GetChamberId();
        var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDate = to ?? DateTime.UtcNow;
        var report = await _service.GetProfitLossAsync(chamberId, fromDate, toDate);
        return Ok(ApiResponse<ProfitLossDto>.Ok(report));
    }

    [HttpGet("reports/monthly")]
    public async Task<ActionResult<ApiResponse<MonthlyReportDto>>> GetMonthlyReport([FromQuery] int? year)
    {
        var chamberId = GetChamberId();
        var reportYear = year ?? DateTime.UtcNow.Year;
        var report = await _service.GetMonthlyReportAsync(chamberId, reportYear);
        return Ok(ApiResponse<MonthlyReportDto>.Ok(report));
    }

    [HttpGet("reports/balance-sheet")]
    public async Task<ActionResult<ApiResponse<BalanceSheetDto>>> GetBalanceSheet([FromQuery] DateTime? asOf)
    {
        var chamberId = GetChamberId();
        var asOfDate = asOf ?? DateTime.UtcNow;
        var report = await _service.GetBalanceSheetAsync(chamberId, asOfDate);
        return Ok(ApiResponse<BalanceSheetDto>.Ok(report));
    }

    [HttpGet("journals")]
    public async Task<ActionResult<ApiResponse<object>>> GetJournals(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] Guid? accountId = null)
    {
        var chamberId = GetChamberId();
        var (items, totalCount) = await _service.GetJournalsAsync(chamberId, page, pageSize, from, to, accountId);
        return Ok(new
        {
            success = true,
            message = "Journals retrieved",
            data = items,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("journals/{id}")]
    public async Task<ActionResult<ApiResponse<JournalResponseDto>>> GetJournalById(Guid id)
    {
        var journal = await _service.GetJournalByIdAsync(id);
        if (journal is null)
            return NotFound(ApiResponse<JournalResponseDto>.Fail("Journal not found"));
        return Ok(ApiResponse<JournalResponseDto>.Ok(journal));
    }

    [HttpPost("journals")]
    public async Task<ActionResult<ApiResponse<JournalResponseDto>>> CreateJournal([FromBody] CreateJournalDto dto)
    {
        try
        {
            var userId = GetUserId();
            var chamberId = GetChamberId();
            var journal = await _service.CreateJournalAsync(dto, userId, chamberId);
            return CreatedAtAction(nameof(GetJournalById), new { id = journal.Id },
                ApiResponse<JournalResponseDto>.Created(journal));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<JournalResponseDto>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<JournalResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPut("journals/{id}")]
    public async Task<ActionResult<ApiResponse<JournalResponseDto>>> UpdateJournal(Guid id, [FromBody] CreateJournalDto dto)
    {
        try
        {
            var journal = await _service.UpdateJournalAsync(id, dto);
            return Ok(ApiResponse<JournalResponseDto>.Ok(journal));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<JournalResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<JournalResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("journals/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteJournal(Guid id)
    {
        try
        {
            await _service.DeleteJournalAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Journal deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
