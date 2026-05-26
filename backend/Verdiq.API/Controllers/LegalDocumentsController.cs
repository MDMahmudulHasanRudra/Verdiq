using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.LegalDocument;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/legal-documents")]
[Authorize]
public class LegalDocumentsController : BaseController
{
    private readonly ILegalDocumentService _legalDocumentService;

    public LegalDocumentsController(ILegalDocumentService legalDocumentService)
    {
        _legalDocumentService = legalDocumentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LegalDocumentResponseDto>>> Create([FromBody] CreateLegalDocumentDto dto)
    {
        var (success, message, data) = await _legalDocumentService.CreateAsync(dto);

        if (!success)
            return BadRequest(ApiResponse<LegalDocumentResponseDto>.Fail(message));

        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<LegalDocumentResponseDto>.Created(data));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<LegalDocumentResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var documents = await _legalDocumentService.GetAllAsync(page, pageSize);
        var list = documents.ToList();

        return Ok(new PagedResponse<LegalDocumentResponseDto>
        {
            Data = list,
            Page = page,
            PageSize = pageSize,
            TotalCount = list.Count,
            TotalPages = (int)Math.Ceiling(list.Count / (double)pageSize)
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LegalDocumentResponseDto>>>> Search([FromQuery] string q)
    {
        var documents = await _legalDocumentService.SearchAsync(q);
        return Ok(ApiResponse<IEnumerable<LegalDocumentResponseDto>>.Ok(documents));
    }

    [HttpGet("by-category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LegalDocumentResponseDto>>>> GetByCategory(string category)
    {
        var documents = await _legalDocumentService.GetByCategoryAsync(category);
        return Ok(ApiResponse<IEnumerable<LegalDocumentResponseDto>>.Ok(documents));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LegalDocumentResponseDto>>> GetById(Guid id)
    {
        var documents = await _legalDocumentService.GetAllAsync();
        var document = documents.FirstOrDefault(d => d.Id == id);

        if (document is null)
            return NotFound(ApiResponse<LegalDocumentResponseDto>.Fail("Legal document not found"));

        return Ok(ApiResponse<LegalDocumentResponseDto>.Ok(document));
    }
}
