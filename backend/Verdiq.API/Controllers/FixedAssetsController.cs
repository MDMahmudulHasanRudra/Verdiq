using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.FixedAsset;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/fixed-assets")]
[Authorize]
public class FixedAssetsController : BaseController
{
    private readonly IFixedAssetService _service;
    public FixedAssetsController(IFixedAssetService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FixedAssetResponseDto>>>> GetAssets()
        => Ok(ApiResponse<List<FixedAssetResponseDto>>.Ok(await _service.GetAssetsAsync(GetChamberId())));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FixedAssetResponseDto>>> GetAsset(Guid id)
    {
        var a = await _service.GetAssetByIdAsync(id);
        return a is null ? NotFound(ApiResponse<FixedAssetResponseDto>.Fail("Not found"))
            : Ok(ApiResponse<FixedAssetResponseDto>.Ok(a));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FixedAssetResponseDto>>> CreateAsset([FromBody] CreateFixedAssetDto dto)
        => Ok(ApiResponse<FixedAssetResponseDto>.Ok(await _service.CreateAssetAsync(dto, GetChamberId())));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FixedAssetResponseDto>>> UpdateAsset(Guid id, [FromBody] CreateFixedAssetDto dto)
        => Ok(ApiResponse<FixedAssetResponseDto>.Ok(await _service.UpdateAssetAsync(id, dto)));

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAsset(Guid id)
    {
        await _service.DeleteAssetAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Asset deleted"));
    }

    [HttpPost("{id}/dispose")]
    public async Task<ActionResult<ApiResponse<FixedAssetResponseDto>>> DisposeAsset(Guid id, [FromBody] DisposeAssetDto dto)
        => Ok(ApiResponse<FixedAssetResponseDto>.Ok(await _service.DisposeAssetAsync(id, dto.DisposalDate, dto.Reason)));
}

public class DisposeAssetDto
{
    public DateTime DisposalDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
