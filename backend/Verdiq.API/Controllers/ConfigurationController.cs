using System.Text.Json;
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
    public async Task<IActionResult> Update([FromBody] JsonElement payload)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        var dto = ParseDto(payload);
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

    private static UpdateChamberSettingsDto ParseDto(JsonElement payload)
    {
        var dto = new UpdateChamberSettingsDto();
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return dto;
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var property in payload.EnumerateObject())
        {
            var key = property.Name.ToLowerInvariant();
            switch (key)
            {
                case "general":
                    dto.General = DeserializeSection(property.Value, options);
                    break;
                case "branding":
                    dto.Branding = DeserializeSection(property.Value, options);
                    break;
                case "casedefaults":
                    dto.CaseDefaults = DeserializeSection(property.Value, options);
                    break;
                case "clientmanagement":
                    dto.ClientManagement = DeserializeSection(property.Value, options);
                    break;
                case "billing":
                    dto.Billing = DeserializeSection(property.Value, options);
                    break;
                case "documentmanagement":
                    dto.DocumentManagement = DeserializeSection(property.Value, options);
                    break;
                case "hearingsreminders":
                    dto.HearingsReminders = DeserializeSection(property.Value, options);
                    break;
                case "legaldrafting":
                    dto.LegalDrafting = DeserializeSection(property.Value, options);
                    break;
                case "communications":
                    dto.Communications = DeserializeSection(property.Value, options);
                    break;
                case "notifications":
                    dto.Notifications = DeserializeSection(property.Value, options);
                    break;
                case "aiassistant":
                    dto.AiAssistant = DeserializeSection(property.Value, options);
                    break;
                case "securitysession":
                    dto.SecuritySession = DeserializeSection(property.Value, options);
                    break;
                case "dashboardui":
                    dto.DashboardUi = DeserializeSection(property.Value, options);
                    break;
                case "integrations":
                    dto.Integrations = DeserializeSection(property.Value, options);
                    break;
                case "dataretention":
                    dto.DataRetention = DeserializeSection(property.Value, options);
                    break;
                case "workflow":
                    dto.Workflow = DeserializeSection(property.Value, options);
                    break;
                default:
                    if (dto.General == null)
                    {
                        dto.General = new Dictionary<string, object>();
                    }

                    dto.General[property.Name] = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number => property.Value.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null!,
                        _ => property.Value.GetRawText()
                    };
                    break;
            }
        }

        return dto;
    }

    private static Dictionary<string, object>? DeserializeSection(JsonElement value, JsonSerializerOptions options)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, object>>(value.GetRawText(), options);
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
