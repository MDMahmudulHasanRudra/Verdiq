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
        [FromQuery] Guid? caseId = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var documents = await _documentService.GetAllDocumentsAsync(caseId, category);

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
        using var stream = file.OpenReadStream();
        var document = await _documentService.UploadDocumentAsync(
            stream, file.FileName, file.ContentType, file.Length,
            documentType, category, caseId, userId);

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
}
