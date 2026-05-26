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
        var chamberId = userId;
        var (success, message, data) = await _taskService.CreateAsync(dto, userId, chamberId);

        if (!success)
            return BadRequest(ApiResponse<TaskResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<TaskResponseDto>.Created(data));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetAll()
    {
        var chamberId = GetUserId();
        var tasks = await _taskService.GetAllAsync(chamberId);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetMyTasks()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetMyTasksAsync(userId);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetByCase(Guid caseId)
    {
        var tasks = await _taskService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> GetById(Guid id)
    {
        var tasks = await _taskService.GetAllAsync(Guid.Empty);
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail("Task not found"));

        return Ok(ApiResponse<TaskResponseDto>.Ok(task));
    }
}
