using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/cases/{caseId}/photos")]
[Authorize]
public class CasePhotosController : BaseController
{
    private readonly ICasePhotoService _photoService;

    public CasePhotosController(ICasePhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CasePhotoDto>>>> GetAll(Guid caseId)
    {
        var photos = await _photoService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<CasePhotoDto>>.Ok(photos.ToList()));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ApiResponse<CasePhotoDto>>> Upload(Guid caseId, IFormFile file, [FromForm] string? caption)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<CasePhotoDto>.Fail("No file provided"));

        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var (success, message, data) = await _photoService.UploadAsync(
            caseId, userId, stream, file.FileName, file.ContentType, caption);
        if (!success)
            return BadRequest(ApiResponse<CasePhotoDto>.Fail(message));
        return Ok(ApiResponse<CasePhotoDto>.Created(data!));
    }

    [HttpGet("{photoId}/download")]
    public async Task<ActionResult> Download(Guid caseId, Guid photoId)
    {
        var (fileStream, contentType, fileName) = await _photoService.DownloadAsync(photoId);
        if (fileStream is null)
            return NotFound(ApiResponse<object>.Fail("Photo not found"));
        return File(fileStream, contentType ?? "application/octet-stream", fileName ?? "photo");
    }

    [HttpDelete("{photoId}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid caseId, Guid photoId)
    {
        var (success, message) = await _photoService.DeleteAsync(photoId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
