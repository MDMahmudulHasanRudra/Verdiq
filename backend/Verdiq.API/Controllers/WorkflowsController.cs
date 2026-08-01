using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.WorkflowProcess;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowsController : BaseController
{
    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WorkflowDto>>>> GetAll()
    {
        var workflows = await _workflowService.GetAllAsync(GetChamberId());
        return Ok(ApiResponse<List<WorkflowDto>>.Ok(workflows.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkflowDto>>> GetById(Guid id)
    {
        var workflow = await _workflowService.GetByIdAsync(id, GetChamberId());
        if (workflow is null)
            return NotFound(ApiResponse<WorkflowDto>.Fail("Workflow not found"));
        return Ok(ApiResponse<WorkflowDto>.Ok(workflow));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkflowDto>>> Create([FromBody] CreateWorkflowDto dto)
    {
        var (success, message, data) = await _workflowService.CreateAsync(dto, GetChamberId(), GetUserId());
        if (!success)
            return BadRequest(ApiResponse<WorkflowDto>.Fail(message));
        return Ok(ApiResponse<WorkflowDto>.Created(data!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkflowDto>>> Update(Guid id, [FromBody] UpdateWorkflowDto dto)
    {
        var (success, message, data) = await _workflowService.UpdateAsync(id, dto, GetChamberId());
        if (!success)
            return BadRequest(ApiResponse<WorkflowDto>.Fail(message));
        return Ok(ApiResponse<WorkflowDto>.Ok(data!));
    }

    [HttpPut("{id}/active")]
    public async Task<ActionResult<ApiResponse<WorkflowDto>>> SetActive(Guid id, [FromQuery] bool isActive)
    {
        var (success, message, data) = await _workflowService.SetActiveAsync(id, isActive, GetChamberId());
        if (!success)
            return NotFound(ApiResponse<WorkflowDto>.Fail(message));
        return Ok(ApiResponse<WorkflowDto>.Ok(data!, message));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _workflowService.DeleteAsync(id, GetChamberId());
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}

[ApiController]
[Route("api/cases/{caseId}/workflows")]
[Authorize]
public class CaseWorkflowsController : BaseController
{
    private readonly IWorkflowService _workflowService;

    public CaseWorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CaseWorkflowDto>>>> GetByCase(Guid caseId)
    {
        var workflows = await _workflowService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<CaseWorkflowDto>>.Ok(workflows.ToList()));
    }

    [HttpGet("{workflowId}")]
    public async Task<ActionResult<ApiResponse<CaseWorkflowDto>>> GetDetail(Guid caseId, Guid workflowId)
    {
        var workflow = await _workflowService.GetCaseWorkflowAsync(caseId, workflowId);
        if (workflow is null)
            return NotFound(ApiResponse<CaseWorkflowDto>.Fail("Workflow not found for this case"));
        return Ok(ApiResponse<CaseWorkflowDto>.Ok(workflow));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CaseWorkflowDto>>> Link(Guid caseId, [FromBody] LinkWorkflowDto dto)
    {
        var (success, message, data) = await _workflowService.LinkAsync(caseId, dto, GetUserId());
        if (!success)
            return BadRequest(ApiResponse<CaseWorkflowDto>.Fail(message));
        return Ok(ApiResponse<CaseWorkflowDto>.Created(data!));
    }

    [HttpPost("{workflowId}/steps/{stepId}/start")]
    public async Task<ActionResult<ApiResponse<object>>> StartStep(Guid caseId, Guid workflowId, Guid stepId)
    {
        var (success, message) = await _workflowService.StartStepAsync(caseId, workflowId, stepId, GetUserId());
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("{workflowId}/steps/{stepId}/complete")]
    public async Task<ActionResult<ApiResponse<object>>> CompleteStep(Guid caseId, Guid workflowId, Guid stepId, [FromBody] CompleteStepDto? dto)
    {
        var (success, message) = await _workflowService.CompleteStepAsync(caseId, workflowId, stepId, dto?.Notes, GetUserId());
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("{workflowId}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid caseId, Guid workflowId)
    {
        var (success, message) = await _workflowService.CancelAsync(caseId, workflowId);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpDelete("{workflowId}")]
    public async Task<ActionResult<ApiResponse<object>>> Unlink(Guid caseId, Guid workflowId)
    {
        var (success, message) = await _workflowService.UnlinkAsync(caseId, workflowId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
