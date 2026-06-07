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

    [HttpGet("preview/{id}")]
    public async Task<ActionResult> Preview(Guid id)
    {
        var (fileStream, contentType, fileName) = await _documentService.PreviewAsync(id);
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

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> Update(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.UpdateAsync(id, dto, userId);
            return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/favorite")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleFavorite(Guid id)
    {
        var userId = GetUserId();
        await _documentService.ToggleFavoriteAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Favorite toggled"));
    }

    [HttpPost("{id}/share")]
    public async Task<ActionResult<ApiResponse<DocumentShareDto>>> Share(Guid id, [FromBody] ShareDocumentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var share = await _documentService.ShareAsync(id, dto, userId);
            return Ok(ApiResponse<DocumentShareDto>.Ok(share));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentShareDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("share/{shareId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveShare(Guid shareId)
    {
        try
        {
            await _documentService.RemoveShareAsync(shareId);
            return Ok(ApiResponse<object>.Ok(null!, "Share removed"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<ApiResponse<List<DocumentCommentDto>>>> GetComments(Guid id)
    {
        var comments = await _documentService.GetCommentsAsync(id);
        return Ok(ApiResponse<List<DocumentCommentDto>>.Ok(comments.ToList()));
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<ApiResponse<DocumentCommentDto>>> AddComment(Guid id, [FromBody] AddDocumentCommentDto dto)
    {
        var userId = GetUserId();
        var comment = await _documentService.AddCommentAsync(id, dto, userId);
        return Ok(ApiResponse<DocumentCommentDto>.Ok(comment));
    }

    [HttpGet("{id}/activity")]
    public async Task<ActionResult<ApiResponse<List<DocumentActivityDto>>>> GetActivity(Guid id)
    {
        var activity = await _documentService.GetActivityAsync(id);
        return Ok(ApiResponse<List<DocumentActivityDto>>.Ok(activity.ToList()));
    }

    [HttpPost("{id}/view")]
    public async Task<ActionResult<ApiResponse<object>>> RecordView(Guid id)
    {
        var userId = GetUserId();
        await _documentService.RecordViewAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPost("{id}/download-count")]
    public async Task<ActionResult<ApiResponse<object>>> RecordDownload(Guid id)
    {
        await _documentService.RecordDownloadAsync(id);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> Search(
        [FromQuery] string q, int page = 1, int pageSize = 20)
    {
        var chamberId = GetChamberId();
        var documents = await _documentService.SearchAsync(chamberId, q, page, pageSize);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> GetRecent(int count = 10)
    {
        var chamberId = GetChamberId();
        var documents = await _documentService.GetRecentAsync(chamberId, count);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponseDto>>>> GetFavorites()
    {
        var userId = GetUserId();
        var documents = await _documentService.GetFavoritesAsync(userId);
        return Ok(ApiResponse<List<DocumentResponseDto>>.Ok(documents.ToList()));
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<ApiResponse<object>>> BulkDelete([FromBody] List<Guid> ids)
    {
        var (success, message) = await _documentService.BulkDeleteAsync(ids);
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("bulk-status")]
    public async Task<ActionResult<ApiResponse<object>>> BulkUpdateStatus([FromBody] BulkStatusDto dto)
    {
        var (success, message) = await _documentService.BulkUpdateStatusAsync(dto.Ids, dto.Status);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("bulk-category")]
    public async Task<ActionResult<ApiResponse<object>>> BulkUpdateCategory([FromBody] BulkCategoryDto dto)
    {
        var (success, message) = await _documentService.BulkUpdateCategoryAsync(dto.Ids, dto.Category);
        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<List<DocumentTemplateDto>>>> GetTemplates(string? category = null)
    {
        var chamberId = GetChamberId();
        var templates = await _documentService.GetTemplatesAsync(chamberId, category);
        return Ok(ApiResponse<List<DocumentTemplateDto>>.Ok(templates.ToList()));
    }

    [HttpPost("templates")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> CreateTemplate(
        [FromForm] CreateDocumentTemplateDto dto, IFormFile? file)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        Stream? fileStream = null;
        if (file != null) fileStream = file.OpenReadStream();
        var template = await _documentService.CreateTemplateAsync(dto, chamberId, userId, fileStream, file?.FileName, file?.ContentType);
        return Ok(ApiResponse<DocumentTemplateDto>.Ok(template));
    }

    [HttpPost("from-template/{templateId}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> CreateFromTemplate(Guid templateId, [FromQuery] Guid caseId)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.CreateFromTemplateAsync(templateId, caseId, userId);
            return Ok(ApiResponse<DocumentResponseDto>.Ok(document));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet("templates/{templateId}/download")]
    public async Task<ActionResult> DownloadTemplate(Guid templateId)
    {
        var (fileStream, contentType, fileName) = await _documentService.DownloadTemplateAsync(templateId);
        if (fileStream is null)
            return NotFound(ApiResponse<object>.Fail("Template not found"));
        return File(fileStream, contentType ?? "application/octet-stream", fileName ?? "template");
    }
}

public class BulkStatusDto
{
    public List<Guid> Ids { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BulkCategoryDto
{
    public List<Guid> Ids { get; set; } = new();
    public string Category { get; set; } = string.Empty;
}
