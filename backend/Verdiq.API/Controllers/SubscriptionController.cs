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
        try
        {
            var userId = GetUserId();
            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
            return Ok(ApiResponse<SubscriptionResponseDto>.Ok(subscription));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SubscriptionResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPut("change-plan")]
    public async Task<ActionResult<ApiResponse<SubscriptionResponseDto>>> ChangePlan([FromBody] ChangePlanDto dto)
    {
        try
        {
            var userId = GetUserId();
            var subscription = await _subscriptionService.ChangePlanAsync(userId, dto.Plan);
            return Ok(ApiResponse<SubscriptionResponseDto>.Ok(subscription));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SubscriptionResponseDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<SubscriptionResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel()
    {
        try
        {
            var userId = GetUserId();
            await _subscriptionService.CancelSubscriptionAsync(userId);
            return Ok(ApiResponse<object>.Ok(null!, "Subscription will cancel at period end"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<SubscriptionResponseDto>>>> GetAll()
    {
        var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        return Ok(ApiResponse<List<SubscriptionResponseDto>>.Ok(subscriptions.ToList()));
    }
}
