using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Verdiq.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    protected Guid GetChamberId() =>
        Guid.Parse(User.FindFirst("chamberId")!.Value);

    protected string GetUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? "";

    protected bool IsAdmin() => GetUserRole() == "Admin";

    protected string? GetIpAddress() =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString();
}
