using Verdiq.Application.DTOs.FixedAsset;

namespace Verdiq.Application.Interfaces;

public interface IFixedAssetService
{
    Task<FixedAssetResponseDto> CreateAssetAsync(CreateFixedAssetDto dto, Guid chamberId);
    Task<FixedAssetResponseDto> UpdateAssetAsync(Guid id, CreateFixedAssetDto dto);
    Task DeleteAssetAsync(Guid id);
    Task<List<FixedAssetResponseDto>> GetAssetsAsync(Guid chamberId);
    Task<FixedAssetResponseDto?> GetAssetByIdAsync(Guid id);
    Task<FixedAssetResponseDto> DisposeAssetAsync(Guid id, DateTime disposalDate, string reason);
}
