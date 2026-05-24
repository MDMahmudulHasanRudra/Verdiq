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
    public async Task<ActionResult<PagedResponse<DocumentResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? tag = null,
        [FromQuery] Guid? caseId = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var documents = await _documentService.GetAllDocumentsAsync(caseId, category, tag);

        if (!string.IsNullOrWhiteSpace(search))
            documents = documents.Where(d =>
                d.OriginalFileName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.CaseTitle.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(status))
            documents = documents.Where(d => d.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        documents = sortBy.ToLower() switch
        {
            "name" => sortOrder == "asc" ? documents.OrderBy(d => d.OriginalFileName) : documents.OrderByDescending(d => d.OriginalFileName),
            "size" => sortOrder == "asc" ? documents.OrderBy(d => d.FileSize) : documents.OrderByDescending(d => d.FileSize),
            _ => sortOrder == "asc" ? documents.OrderBy(d => d.CreatedAt) : documents.OrderByDescending(d => d.CreatedAt),
        };

        var totalCount = documents.Count();
        var paged = documents.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PagedResponse<DocumentResponseDto>
        {
            Data = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> GetByCase(Guid caseId)
    {
        var documents = await _documentService.GetDocumentsByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> GetById(Guid id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> Upload(
        [FromQuery] Guid caseId,
        [FromQuery] string documentType,
        [FromQuery] string category,
        [FromQuery] string? tags,
        IFormFile file)
    {
        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };

        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<DocumentResponseDto>.Fail("No file provided"));

        if (!allowedTypes.Contains(file.ContentType) &&
            !file.FileName.EndsWith(".pdf") && !file.FileName.EndsWith(".doc") &&
            !file.FileName.EndsWith(".docx") && !file.FileName.EndsWith(".xlsx") &&
            !file.FileName.EndsWith(".jpg") && !file.FileName.EndsWith(".jpeg") &&
            !file.FileName.EndsWith(".png"))
        {
            return BadRequest(ApiResponse<DocumentResponseDto>.Fail("File type not allowed"));
        }

        if (file.Length > 50_000_000)
            return BadRequest(ApiResponse<DocumentResponseDto>.Fail("File size exceeds 50MB limit"));

        var userId = GetUserId();
        var tagList = !string.IsNullOrWhiteSpace(tags)
            ? tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : null;

        using var stream = file.OpenReadStream();
        var document = await _documentService.UploadDocumentAsync(
            stream, file.FileName, file.ContentType, file.Length,
            documentType, category, caseId, userId, tagList);

        return Ok(ApiResponse<DocumentResponseDto>.Created(document));
    }

    [HttpGet("download/{id}")]
    public async Task<ActionResult> Download(Guid id)
    {
        try
        {
            var (fileStream, contentType, fileName) = await _documentService.DownloadDocumentAsync(id);
            return File(fileStream, contentType, fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _documentService.DeleteDocumentAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Document deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkDelete([FromBody] List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest(ApiResponse<BulkOperationResult>.Fail("No document IDs provided"));

        var result = await _documentService.BulkDeleteAsync(ids);
        return Ok(ApiResponse<BulkOperationResult>.Ok(result));
    }

    [HttpGet("{id}/signed-url")]
    public async Task<ActionResult<ApiResponse<string>>> GetSignedUrl(Guid id)
    {
        try
        {
            var url = await _documentService.GenerateSignedUrlAsync(id);
            return Ok(ApiResponse<string>.Ok(url));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/versions")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> UploadVersion(
        Guid id,
        [FromQuery] string? changeNotes,
        IFormFile file)
    {
        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var document = await _documentService.UploadNewVersionAsync(
            id, stream, file.FileName, file.ContentType, file.Length, userId, changeNotes);
        return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult<ApiResponse<List<DocumentVersionDto>>>> GetVersions(Guid id)
    {
        var versions = await _documentService.GetVersionHistoryAsync(id);
        return Ok(ApiResponse<List<DocumentVersionDto>>.Ok(versions));
    }

    [HttpPost("{id}/versions/{versionId}/restore")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> RestoreVersion(Guid id, Guid versionId)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.RestoreVersionAsync(id, versionId, userId);
            return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/tags")]
    public async Task<ActionResult<ApiResponse<object>>> AddTag(Guid id, [FromBody] AddTagRequest request)
    {
        try
        {
            await _documentService.AddTagAsync(id, request.TagName);
            return Ok(ApiResponse<object>.Ok(null!, "Tag added"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}/tags/{tagName}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveTag(Guid id, string tagName)
    {
        try
        {
            await _documentService.RemoveTagAsync(id, tagName);
            return Ok(ApiResponse<object>.Ok(null!, "Tag removed"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}/tags")]
    public async Task<ActionResult<ApiResponse<List<DocumentTagDto>>>> GetTags(Guid id)
    {
        var tags = await _documentService.GetTagsAsync(id);
        return Ok(ApiResponse<List<DocumentTagDto>>.Ok(tags));
    }
}

public class AddTagRequest
{
    public string TagName { get; set; } = string.Empty;
}
