using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.Application.DTOs.LegalSection;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/legal-sections")]
[Authorize]
public class LegalSectionsController : ControllerBase
{
    private readonly ILegalSectionService _service;
    private readonly IHttpContextAccessor _httpContext;

    public LegalSectionsController(ILegalSectionService service, IHttpContextAccessor httpContext)
    {
        _service = service;
        _httpContext = httpContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        var chamberId = GetChamberId();
        var data = await _service.GetAllAsync(chamberId, category, search);
        return Ok(new { success = true, data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null) return NotFound(new { success = false, message = "Legal section not found" });
        return Ok(new { success = true, data });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var chamberId = GetChamberId();
        var data = await _service.SearchAsync(q, chamberId);
        return Ok(new { success = true, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLegalSectionDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _service.CreateAsync(dto, chamberId);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLegalSectionDto dto)
    {
        var (success, message, data) = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { success, message });
        return Ok(new { success, message });
    }

    private Guid GetChamberId()
    {
        var claim = _httpContext.HttpContext?.User.FindFirst("chamberId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
