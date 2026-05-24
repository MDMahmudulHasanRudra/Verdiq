using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Notification;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetAll([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(notifications.ToList()));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationResponseDto>>> Create([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotificationAsync(dto);
        return Ok(ApiResponse<NotificationResponseDto>.Created(notification));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Notification marked as read"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _notificationService.DeleteNotificationAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Notification deleted"));
    }
}
