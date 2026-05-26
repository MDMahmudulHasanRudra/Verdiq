using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Subscription;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<SubscriptionResponseDto>>> GetMySubscription()
    {
        var chamberId = GetChamberId();
        var subscription = await _subscriptionService.GetByChamberIdAsync(chamberId);
        if (subscription == null)
            return NotFound(ApiResponse<SubscriptionResponseDto>.Fail("Subscription not found"));

        return Ok(ApiResponse<SubscriptionResponseDto>.Ok(subscription));
    }

    [HttpPut("change-plan")]
    public async Task<ActionResult<ApiResponse<SubscriptionResponseDto>>> ChangePlan(
        [FromBody] ChangePlanDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message) = await _subscriptionService.ChangePlanAsync(chamberId, dto.Plan);

        if (!success)
            return BadRequest(ApiResponse<SubscriptionResponseDto>.Fail(message));

        var subscription = await _subscriptionService.GetByChamberIdAsync(chamberId);
        return Ok(ApiResponse<SubscriptionResponseDto>.Ok(subscription!, message));
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel()
    {
        var chamberId = GetChamberId();
        var (success, message) = await _subscriptionService.CancelAsync(chamberId);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, "Subscription will cancel at period end"));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<SubscriptionResponseDto>>>> GetAll()
    {
        var subscriptions = await _subscriptionService.GetAllAsync();
        return Ok(ApiResponse<List<SubscriptionResponseDto>>.Ok(subscriptions.ToList()));
    }
}
