using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Document;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : BaseController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> GetAll(
        int page = 1, int pageSize = 10, string? category = null)
    {
        var chamberId = GetChamberId();
        var documents = await _documentService.GetAllAsync(chamberId, category, page, pageSize);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> GetByCase(Guid caseId)
    {
        var documents = await _documentService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> GetById(Guid id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document is null)
            return NotFound(ApiResponse<DocumentResponseDto>.Fail("Document not found"));
        return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> Upload(
        [FromQuery] Guid caseId,
        [FromQuery] string category,
        [FromQuery] string? folderPath,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<DocumentResponseDto>.Fail("No file provided"));

        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var (success, message, data) = await _documentService.UploadAsync(
            caseId, userId, category, folderPath, stream, file.FileName, file.ContentType);

        if (!success)
            return BadRequest(ApiResponse<DocumentResponseDto>.Fail(message));
        return Ok(ApiResponse<DocumentResponseDto>.Created(data!));
    }

    [HttpGet("download/{id}")]
    public async Task<ActionResult> Download(Guid id)
    {
        var (fileStream, contentType, fileName) = await _documentService.DownloadAsync(id);
        if (fileStream is null)
            return NotFound(ApiResponse<object>.Fail("Document not found"));
        return File(fileStream, contentType!, fileName!);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _documentService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
