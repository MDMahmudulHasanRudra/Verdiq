using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/cases/{caseId}/judgments")]
[Authorize]
public class JudgmentsController : BaseController
{
    private readonly IJudgmentService _judgmentService;

    public JudgmentsController(IJudgmentService judgmentService)
    {
        _judgmentService = judgmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JudgmentDto>>>> GetAll(Guid caseId)
    {
        var judgments = await _judgmentService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<JudgmentDto>>.Ok(judgments.ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<JudgmentDto>>> Create(Guid caseId, [FromBody] CreateJudgmentDto dto)
    {
        var userId = GetUserId();
        var (success, message, data) = await _judgmentService.CreateAsync(caseId, dto, userId);
        if (!success)
            return BadRequest(ApiResponse<JudgmentDto>.Fail(message));
        return Ok(ApiResponse<JudgmentDto>.Created(data!));
    }

    [HttpPost("{judgmentId}/upload-document")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<JudgmentDto>>> UploadDocument(Guid caseId, Guid judgmentId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<JudgmentDto>.Fail("No file provided"));

        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var (success, message, data) = await _judgmentService.UploadDocumentAsync(
            caseId, judgmentId, userId, stream, file.FileName, file.ContentType);
        if (!success)
            return BadRequest(ApiResponse<JudgmentDto>.Fail(message));
        return Ok(ApiResponse<JudgmentDto>.Ok(data!, message));
    }

    [HttpGet("{judgmentId}/download-document")]
    public async Task<ActionResult> DownloadDocument(Guid caseId, Guid judgmentId)
    {
        var (fileStream, contentType, fileName) = await _judgmentService.DownloadDocumentAsync(caseId, judgmentId);
        if (fileStream is null)
            return NotFound(ApiResponse<object>.Fail("No document attached to this judgment"));
        return File(fileStream, contentType ?? "application/octet-stream", fileName ?? "judgment-document");
    }

    [HttpGet("export")]
    public async Task<ActionResult> Export(Guid caseId, string format = "pdf")
    {
        var (content, contentType, fileName) = await _judgmentService.ExportAsync(caseId, format);
        return File(content, contentType, fileName);
    }

    [HttpDelete("{judgmentId}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid caseId, Guid judgmentId)
    {
        var (success, message) = await _judgmentService.DeleteAsync(judgmentId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
