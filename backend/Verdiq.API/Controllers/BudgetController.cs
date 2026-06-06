using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Budget;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/budget")]
[Authorize]
public class BudgetController : BaseController
{
    private readonly IBudgetService _service;
    public BudgetController(IBudgetService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BudgetResponseDto>>>> GetBudgets([FromQuery] int? fiscalYear)
        => Ok(ApiResponse<List<BudgetResponseDto>>.Ok(await _service.GetBudgetsAsync(GetChamberId(), fiscalYear)));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> GetBudget(Guid id)
    {
        var b = await _service.GetBudgetByIdAsync(id);
        return b is null ? NotFound(ApiResponse<BudgetResponseDto>.Fail("Not found"))
            : Ok(ApiResponse<BudgetResponseDto>.Ok(b));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> CreateBudget([FromBody] CreateBudgetDto dto)
        => Ok(ApiResponse<BudgetResponseDto>.Ok(await _service.CreateBudgetAsync(dto, GetUserId(), GetChamberId())));

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> ApproveBudget(Guid id)
        => Ok(ApiResponse<BudgetResponseDto>.Ok(await _service.ApproveBudgetAsync(id)));
}
