using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Task;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Create([FromBody] CreateTaskDto dto)
    {
        var userId = GetUserId();
        var chamberId = GetChamberId();
        var (success, message, data) = await _taskService.CreateAsync(dto, userId, chamberId);

        if (!success)
            return BadRequest(ApiResponse<TaskResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<TaskResponseDto>.Created(data));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskResponseDto>>>> GetAll(
        string? status = null, string? priority = null, Guid? assignedTo = null)
    {
        var chamberId = GetChamberId();
        var tasks = await _taskService.GetAllAsync(chamberId, status, priority, assignedTo);
        return Ok(ApiResponse<List<TaskResponseDto>>.Ok(tasks.ToList()));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<TaskResponseDto>>>> GetMyTasks()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetMyTasksAsync(userId);
        return Ok(ApiResponse<List<TaskResponseDto>>.Ok(tasks.ToList()));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<List<TaskResponseDto>>>> GetByCase(Guid caseId)
    {
        var tasks = await _taskService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<TaskResponseDto>>.Ok(tasks.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail("Task not found"));
        return Ok(ApiResponse<TaskResponseDto>.Ok(task));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var (success, message, data) = await _taskService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(ApiResponse<TaskResponseDto>.Fail(message));
        return Ok(ApiResponse<TaskResponseDto>.Ok(data!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _taskService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<List<TaskResponseDto>>>> GetOverdue()
    {
        var chamberId = GetChamberId();
        var tasks = await _taskService.GetOverdueAsync(chamberId);
        return Ok(ApiResponse<List<TaskResponseDto>>.Ok(tasks.ToList()));
    }

    [HttpPost("reorder")]
    public async Task<ActionResult<ApiResponse<object>>> Reorder([FromBody] ReorderTasksDto dto)
    {
        await _taskService.ReorderAsync(dto);
        return Ok(ApiResponse<object>.Ok(null!, "Tasks reordered"));
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<ApiResponse<TaskCommentDto>>> AddComment(Guid id, [FromBody] AddTaskCommentDto dto)
    {
        var userId = GetUserId();
        var comment = await _taskService.AddCommentAsync(id, dto, userId);
        return Ok(ApiResponse<TaskCommentDto>.Ok(comment));
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<ApiResponse<List<TaskCommentDto>>>> GetComments(Guid id)
    {
        var comments = await _taskService.GetCommentsAsync(id);
        return Ok(ApiResponse<List<TaskCommentDto>>.Ok(comments.ToList()));
    }

    [HttpPost("{id}/watchers")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleWatcher(Guid id)
    {
        var userId = GetUserId();
        var isWatching = await _taskService.ToggleWatcherAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(null!, isWatching ? "Now watching" : "No longer watching"));
    }

    [HttpPost("{id}/start-timer")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> StartTimer(Guid id)
    {
        try
        {
            var task = await _taskService.StartTimeTrackingAsync(id);
            return Ok(ApiResponse<TaskResponseDto>.Ok(task));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaskResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/stop-timer")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> StopTimer(Guid id, [FromBody] StopTimerDto dto)
    {
        try
        {
            var task = await _taskService.StopTimeTrackingAsync(id, dto.Minutes);
            return Ok(ApiResponse<TaskResponseDto>.Ok(task));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaskResponseDto>.Fail(ex.Message));
        }
    }
}

public class StopTimerDto
{
    public double Minutes { get; set; }
}
