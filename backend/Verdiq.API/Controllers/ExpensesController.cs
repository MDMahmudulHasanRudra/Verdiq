using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Expense;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseResponseDto>>> Create([FromBody] CreateExpenseDto dto)
    {
        var userId = GetUserId();
        var chamberId = userId;
        var (success, message, data) = await _expenseService.CreateAsync(dto, userId, chamberId);

        if (!success)
            return BadRequest(ApiResponse<ExpenseResponseDto>.Fail(message));

        return CreatedAtAction(null, ApiResponse<ExpenseResponseDto>.Created(data!));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ExpenseResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null)
    {
        var chamberId = GetUserId();
        var expenses = await _expenseService.GetAllAsync(chamberId, category, page, pageSize);
        var list = expenses.ToList();

        return Ok(new PagedResponse<ExpenseResponseDto>
        {
            Data = list,
            Page = page,
            PageSize = pageSize,
            TotalCount = list.Count,
            TotalPages = (int)Math.Ceiling(list.Count / (double)pageSize)
        });
    }

    [HttpGet("total")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotal()
    {
        var chamberId = GetUserId();
        var total = await _expenseService.GetTotalAsync(chamberId);
        return Ok(ApiResponse<decimal>.Ok(total));
    }
}
