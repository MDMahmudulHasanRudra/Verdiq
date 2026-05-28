using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.Application.DTOs.ChamberSettings;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/configuration")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IChamberSettingsService _service;
    private readonly IHttpContextAccessor _httpContext;

    public ConfigurationController(IChamberSettingsService service, IHttpContextAccessor httpContext)
    {
        _service = service;
        _httpContext = httpContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _service.GetSettingsAsync(chamberId);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpGet("{subsection}")]
    public async Task<IActionResult> GetSubsection(string subsection)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _service.GetSubsectionAsync(chamberId, subsection);
        if (!success) return NotFound(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateChamberSettingsDto dto)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        var (success, message, data) = await _service.UpdateSettingsAsync(chamberId, dto, userId);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpPut("{subsection}")]
    public async Task<IActionResult> UpdateSubsection(string subsection, [FromBody] Dictionary<string, object> values)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        var (success, message, data) = await _service.UpdateSubsectionAsync(chamberId, subsection, values, userId);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    private Guid GetChamberId()
    {
        var claim = _httpContext.HttpContext?.User.FindFirst("chamberId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private Guid GetUserId()
    {
        var claim = _httpContext.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
