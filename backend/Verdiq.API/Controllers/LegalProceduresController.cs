using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.Application.DTOs.LegalProcedure;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/legal-sections/{sectionId}/procedures")]
[Authorize]
public class LegalProceduresController : ControllerBase
{
    private readonly ILegalProcedureService _service;

    public LegalProceduresController(ILegalProcedureService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid sectionId)
    {
        var data = await _service.GetBySectionAsync(sectionId);
        return Ok(new { success = true, data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null) return NotFound(new { success = false, message = "Legal procedure not found" });
        return Ok(new { success = true, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid sectionId, [FromBody] CreateLegalProcedureDto dto)
    {
        dto.LegalSectionId = sectionId;
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLegalProcedureDto dto)
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
}

[ApiController]
[Route("api/cases/{caseId}/procedures")]
[Authorize]
public class CaseProceduresController : ControllerBase
{
    private readonly ILegalProcedureService _service;

    public CaseProceduresController(ILegalProcedureService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetCaseProcedures(Guid caseId)
    {
        var data = await _service.GetCaseProceduresAsync(caseId);
        return Ok(new { success = true, data });
    }

    [HttpPost("generate/{legalSectionId}")]
    public async Task<IActionResult> GenerateProcedures(Guid caseId, Guid legalSectionId)
    {
        var (success, message) = await _service.GenerateCaseProceduresAsync(caseId, legalSectionId);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message });
    }

    [HttpPost("{procedureId}/complete")]
    public async Task<IActionResult> CompleteProcedure(Guid caseId, Guid procedureId)
    {
        var completedBy = User.Identity?.Name ?? "System";
        var (success, message) = await _service.CompleteCaseProcedureAsync(procedureId, completedBy);
        if (!success) return NotFound(new { success, message });
        return Ok(new { success, message });
    }
}
