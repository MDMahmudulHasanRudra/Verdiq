using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Template;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/templates")]
[Authorize]
public class TemplatesController : BaseController
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TemplateResponseDto>>> Create([FromBody] CreateTemplateDto dto)
    {
        var (success, message, data) = await _templateService.CreateAsync(dto);

        if (!success)
            return BadRequest(ApiResponse<TemplateResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<TemplateResponseDto>.Created(data));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TemplateResponseDto>>>> GetAll([FromQuery] string? category)
    {
        var templates = await _templateService.GetAllAsync(category);
        return Ok(ApiResponse<IEnumerable<TemplateResponseDto>>.Ok(templates));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TemplateResponseDto>>> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);

        if (template is null)
            return NotFound(ApiResponse<TemplateResponseDto>.Fail("Template not found"));

        return Ok(ApiResponse<TemplateResponseDto>.Ok(template));
    }

    [HttpPost("{id}/render")]
    public async Task<ActionResult<ApiResponse<string>>> Render(Guid id, [FromBody] Dictionary<string, string> variables)
    {
        try
        {
            var rendered = await _templateService.RenderTemplateAsync(id, variables);
            return Ok(ApiResponse<string>.Ok(rendered));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }
}
