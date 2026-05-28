using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.Application.DTOs.Workflow;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/workflow/templates")]
[Authorize]
public class WorkflowTemplatesController : ControllerBase
{
    private readonly IWorkflowTemplateService _service;
    private readonly IHttpContextAccessor _httpContext;

    public WorkflowTemplatesController(IWorkflowTemplateService service, IHttpContextAccessor httpContext)
    {
        _service = service;
        _httpContext = httpContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var chamberId = GetChamberId();
        var data = await _service.GetAllAsync(chamberId);
        return Ok(new { success = true, data });
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault()
    {
        var chamberId = GetChamberId();
        var data = await _service.GetDefaultAsync(chamberId);
        if (data == null) return NotFound(new { success = false, message = "No default template" });
        return Ok(new { success = true, data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null) return NotFound(new { success = false, message = "Template not found" });
        return Ok(new { success = true, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowTemplateDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _service.CreateAsync(dto, chamberId);
        if (!success) return BadRequest(new { success, message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { success, message, data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkflowTemplateDto dto)
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
