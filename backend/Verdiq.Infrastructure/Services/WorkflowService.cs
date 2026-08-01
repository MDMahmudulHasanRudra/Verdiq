using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.WorkflowProcess;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _context;

    public WorkflowService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<WorkflowDto>> GetAllAsync(Guid chamberId)
    {
        var workflows = await _context.Workflows
            .Include(w => w.CreatedBy)
            .Include(w => w.Steps)
            .Where(w => w.ChamberId == chamberId && !w.IsDeleted)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return workflows.Select(MapToWorkflowDto);
    }

    public async Task<WorkflowDto?> GetByIdAsync(Guid id, Guid chamberId)
    {
        var workflow = await _context.Workflows
            .Include(w => w.CreatedBy)
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id && w.ChamberId == chamberId && !w.IsDeleted);

        return workflow == null ? null : MapToWorkflowDto(workflow);
    }

    public async Task<(bool Success, string Message, WorkflowDto? Data)> CreateAsync(CreateWorkflowDto dto, Guid chamberId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Workflow name is required", null);

        if (dto.Steps == null || dto.Steps.Count == 0)
            return (false, "Add at least one step to the workflow", null);

        if (dto.Steps.Any(s => string.IsNullOrWhiteSpace(s.Title)))
            return (false, "Every step needs a title", null);

        var workflow = new Workflow
        {
            ChamberId = chamberId,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        AddSteps(workflow.Id, workflow.Steps, dto.Steps);

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(workflow.Id, chamberId);
        return (true, "Workflow created", result);
    }

    public async Task<(bool Success, string Message, WorkflowDto? Data)> UpdateAsync(Guid id, UpdateWorkflowDto dto, Guid chamberId)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id && w.ChamberId == chamberId && !w.IsDeleted);

        if (workflow == null)
            return (false, "Workflow not found", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Workflow name is required", null);

        if (dto.Steps == null || dto.Steps.Count == 0)
            return (false, "Add at least one step to the workflow", null);

        if (dto.Steps.Any(s => string.IsNullOrWhiteSpace(s.Title)))
            return (false, "Every step needs a title", null);

        workflow.Name = dto.Name.Trim();
        workflow.Description = dto.Description;
        workflow.UpdatedAt = DateTime.UtcNow;

        foreach (var existing in workflow.Steps.ToList())
        {
            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        var replacement = new List<WorkflowStep>();
        AddSteps(workflow.Id, replacement, dto.Steps);
        _context.WorkflowSteps.AddRange(replacement);

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(workflow.Id, chamberId);
        return (true, "Workflow updated", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid chamberId)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id && w.ChamberId == chamberId && !w.IsDeleted);

        if (workflow == null)
            return (false, "Workflow not found");

        workflow.IsDeleted = true;
        workflow.UpdatedAt = DateTime.UtcNow;

        foreach (var step in workflow.Steps.ToList())
        {
            step.IsDeleted = true;
            step.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, "Workflow deleted");
    }

    public async Task<(bool Success, string Message, WorkflowDto? Data)> SetActiveAsync(Guid id, bool isActive, Guid chamberId)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == id && w.ChamberId == chamberId && !w.IsDeleted);

        if (workflow == null)
            return (false, "Workflow not found", null);

        workflow.IsActive = isActive;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(workflow.Id, chamberId);
        return (true, isActive ? "Workflow activated" : "Workflow deactivated", result);
    }

    public async Task<IEnumerable<CaseWorkflowDto>> GetByCaseIdAsync(Guid caseId)
    {
        var workflows = await _context.CaseWorkflows
            .Include(cw => cw.StartedBy)
            .Include(cw => cw.Steps).ThenInclude(s => s.CompletedBy)
            .Where(cw => cw.CaseId == caseId && !cw.IsDeleted)
            .OrderByDescending(cw => cw.StartedAt)
            .ToListAsync();

        return workflows.Select(MapToCaseWorkflowDto);
    }

    public async Task<CaseWorkflowDto?> GetCaseWorkflowAsync(Guid caseId, Guid caseWorkflowId)
    {
        var workflow = await _context.CaseWorkflows
            .Include(cw => cw.StartedBy)
            .Include(cw => cw.Steps).ThenInclude(s => s.CompletedBy)
            .FirstOrDefaultAsync(cw => cw.Id == caseWorkflowId && cw.CaseId == caseId && !cw.IsDeleted);

        return workflow == null ? null : MapToCaseWorkflowDto(workflow);
    }

    public async Task<(bool Success, string Message, CaseWorkflowDto? Data)> LinkAsync(Guid caseId, LinkWorkflowDto dto, Guid userId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null || caseEntity.IsDeleted)
            return (false, "Case not found", null);

        var workflow = await _context.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == dto.WorkflowId && !w.IsDeleted);

        if (workflow == null)
            return (false, "Workflow not found", null);

        if (workflow.Steps.Count == 0)
            return (false, "This workflow has no steps to run", null);

        var duplicate = await _context.CaseWorkflows
            .AnyAsync(cw => cw.CaseId == caseId && cw.WorkflowId == dto.WorkflowId && !cw.IsDeleted);

        if (duplicate)
            return (false, "This workflow is already linked to the case", null);

        var startedAt = DateTime.UtcNow;
        var caseWorkflow = new CaseWorkflow
        {
            CaseId = caseId,
            WorkflowId = workflow.Id,
            WorkflowName = workflow.Name,
            WorkflowDescription = workflow.Description,
            Status = "InProgress",
            StartedAt = startedAt,
            StartedById = userId,
            CreatedAt = startedAt
        };

        foreach (var step in workflow.Steps.OrderBy(s => s.OrderIndex))
        {
            caseWorkflow.Steps.Add(new CaseWorkflowStep
            {
                StepId = step.Id,
                Title = step.Title,
                Description = step.Description,
                OrderIndex = step.OrderIndex,
                DueInDays = step.DueInDays,
                DueDate = step.DueInDays.HasValue ? startedAt.AddDays(step.DueInDays.Value) : null,
                Status = "Pending",
                CreatedAt = startedAt
            });
        }

        _context.CaseWorkflows.Add(caseWorkflow);
        await _context.SaveChangesAsync();

        AddActivity(caseId, $"Workflow \"{workflow.Name}\" attached to the case", userId);
        await _context.SaveChangesAsync();

        var result = await GetCaseWorkflowAsync(caseId, caseWorkflow.Id);
        return (true, "Workflow linked to case", result);
    }

    public async Task<(bool Success, string Message)> StartStepAsync(Guid caseId, Guid caseWorkflowId, Guid stepId, Guid userId)
    {
        var (caseWorkflow, message) = await LoadActiveAsync(caseId, caseWorkflowId);
        if (caseWorkflow == null)
            return (false, message);

        var active = GetActiveStep(caseWorkflow);
        if (active == null)
            return (false, "All steps are already completed");

        if (active.Id != stepId)
            return (false, $"Complete \"{active.Title}\" first before starting another step");

        if (active.Status != "Pending")
            return (false, "This step has already been started");

        active.Status = "InProgress";
        active.StartedAt = DateTime.UtcNow;
        active.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, $"Step \"{active.Title}\" started");
    }

    public async Task<(bool Success, string Message)> CompleteStepAsync(Guid caseId, Guid caseWorkflowId, Guid stepId, string? notes, Guid userId)
    {
        var (caseWorkflow, message) = await LoadActiveAsync(caseId, caseWorkflowId);
        if (caseWorkflow == null)
            return (false, message);

        var active = GetActiveStep(caseWorkflow);
        if (active == null)
            return (false, "All steps are already completed");

        if (active.Id != stepId)
            return (false, $"Complete \"{active.Title}\" first — the next step unlocks after it");

        var now = DateTime.UtcNow;
        active.Status = "Completed";
        active.StartedAt ??= now;
        active.CompletedAt = now;
        active.CompletedById = userId;
        active.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        active.UpdatedAt = now;

        AddActivity(caseId, $"Workflow \"{caseWorkflow.WorkflowName}\": step \"{active.Title}\" completed", userId);

        var remaining = caseWorkflow.Steps.Where(s => s.Status != "Completed").ToList();
        if (remaining.Count == 0)
        {
            caseWorkflow.Status = "Completed";
            caseWorkflow.CompletedAt = now;
            caseWorkflow.UpdatedAt = now;
            AddActivity(caseId, $"Workflow \"{caseWorkflow.WorkflowName}\" completed", userId);
        }
        else
        {
            var next = remaining.OrderBy(s => s.OrderIndex).First();
            AddActivity(caseId, $"Workflow \"{caseWorkflow.WorkflowName}\": next step \"{next.Title}\" unlocked", userId);
        }

        await _context.SaveChangesAsync();
        return (true, $"Step \"{active.Title}\" completed");
    }

    public async Task<(bool Success, string Message)> CancelAsync(Guid caseId, Guid caseWorkflowId)
    {
        var caseWorkflow = await _context.CaseWorkflows
            .FirstOrDefaultAsync(cw => cw.Id == caseWorkflowId && cw.CaseId == caseId && !cw.IsDeleted);

        if (caseWorkflow == null)
            return (false, "Workflow not found for this case");

        if (caseWorkflow.Status == "Completed")
            return (false, "A completed workflow cannot be cancelled");

        caseWorkflow.Status = "Cancelled";
        caseWorkflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, $"Workflow \"{caseWorkflow.WorkflowName}\" cancelled");
    }

    public async Task<(bool Success, string Message)> UnlinkAsync(Guid caseId, Guid caseWorkflowId)
    {
        var caseWorkflow = await _context.CaseWorkflows
            .Include(cw => cw.Steps)
            .FirstOrDefaultAsync(cw => cw.Id == caseWorkflowId && cw.CaseId == caseId && !cw.IsDeleted);

        if (caseWorkflow == null)
            return (false, "Workflow not found for this case");

        caseWorkflow.IsDeleted = true;
        caseWorkflow.UpdatedAt = DateTime.UtcNow;

        foreach (var step in caseWorkflow.Steps.ToList())
        {
            step.IsDeleted = true;
            step.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, $"Workflow \"{caseWorkflow.WorkflowName}\" removed from the case");
    }

    private async Task<(CaseWorkflow? CaseWorkflow, string Message)> LoadActiveAsync(Guid caseId, Guid caseWorkflowId)
    {
        var caseWorkflow = await _context.CaseWorkflows
            .Include(cw => cw.Steps).ThenInclude(s => s.CompletedBy)
            .FirstOrDefaultAsync(cw => cw.Id == caseWorkflowId && cw.CaseId == caseId && !cw.IsDeleted);

        if (caseWorkflow == null)
            return (null, "Workflow not found for this case");

        if (caseWorkflow.Status == "Completed")
            return (null, "This workflow is already completed");

        if (caseWorkflow.Status == "Cancelled")
            return (null, "This workflow has been cancelled");

        return (caseWorkflow, "");
    }

    private static CaseWorkflowStep? GetActiveStep(CaseWorkflow caseWorkflow) =>
        caseWorkflow.Steps
            .Where(s => s.Status != "Completed")
            .OrderBy(s => s.OrderIndex)
            .FirstOrDefault();
    private static void AddSteps(Guid workflowId, ICollection<WorkflowStep> target, IEnumerable<CreateWorkflowStepDto> steps)
    {
        var index = 0;
        foreach (var s in steps)
        {
            target.Add(new WorkflowStep
            {
                WorkflowId = workflowId,
                Title = s.Title.Trim(),
                Description = s.Description,
                OrderIndex = index++,
                DueInDays = s.DueInDays,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private void AddActivity(Guid caseId, string description, Guid userId)
    {
        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            ActivityType = Domain.Enums.ActivityType.Task,
            Description = description,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static WorkflowDto MapToWorkflowDto(Workflow w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Description = w.Description,
        IsActive = w.IsActive,
        StepCount = w.Steps.Count(s => !s.IsDeleted),
        CreatedByName = w.CreatedBy?.FullName,
        CreatedAt = w.CreatedAt,
        Steps = w.Steps
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.OrderIndex)
            .Select(s => new WorkflowStepItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                OrderIndex = s.OrderIndex,
                DueInDays = s.DueInDays
            })
            .ToList()
    };

    private static CaseWorkflowDto MapToCaseWorkflowDto(CaseWorkflow cw)
    {
        var steps = cw.Steps
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.OrderIndex)
            .ToList();

        var completed = steps.Count(s => s.Status == "Completed");
        var activeStep = cw.Status == "InProgress" ? steps.FirstOrDefault(s => s.Status != "Completed") : null;
        var now = DateTime.UtcNow;

        return new CaseWorkflowDto
        {
            Id = cw.Id,
            CaseId = cw.CaseId,
            WorkflowId = cw.WorkflowId,
            WorkflowName = cw.WorkflowName,
            WorkflowDescription = cw.WorkflowDescription,
            Status = cw.Status,
            StartedAt = cw.StartedAt,
            CompletedAt = cw.CompletedAt,
            StartedByName = cw.StartedBy?.FullName,
            StepCount = steps.Count,
            CompletedStepCount = completed,
            PercentComplete = steps.Count == 0 ? 0 : (int)Math.Round((double)completed / steps.Count * 100),
            IsOverdue = steps.Any(s => s.Status != "Completed" && s.DueDate.HasValue && s.DueDate.Value < now),
            NextStepTitle = activeStep?.Title,
            Steps = steps.Select(s => MapToCaseWorkflowStepDto(s, s.Id == activeStep?.Id))
                .ToList()
        };
    }

    private static CaseWorkflowStepDto MapToCaseWorkflowStepDto(CaseWorkflowStep s, bool isActive) => new()
    {
        Id = s.Id,
        StepId = s.StepId,
        Title = s.Title,
        Description = s.Description,
        OrderIndex = s.OrderIndex,
        DueDate = s.DueDate,
        Status = s.Status,
        StartedAt = s.StartedAt,
        CompletedAt = s.CompletedAt,
        CompletedByName = s.CompletedBy?.FullName,
        Notes = s.Notes,
        IsCompleted = s.Status == "Completed",
        IsActive = isActive,
        IsLocked = s.Status != "Completed" && !isActive,
        IsOverdue = s.Status != "Completed" && s.DueDate.HasValue && s.DueDate.Value < DateTime.UtcNow
    };
}
