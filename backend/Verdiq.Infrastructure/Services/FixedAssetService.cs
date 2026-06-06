using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.FixedAsset;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class FixedAssetService : IFixedAssetService
{
    private readonly AppDbContext _context;
    public FixedAssetService(AppDbContext context) => _context = context;

    public async Task<FixedAssetResponseDto> CreateAssetAsync(CreateFixedAssetDto dto, Guid chamberId)
    {
        var count = await _context.Set<FixedAsset>().CountAsync(a => a.ChamberId == chamberId);
        var method = Enum.Parse<AssetDepreciationMethod>(dto.DepreciationMethod);

        var asset = new FixedAsset
        {
            AssetCode = $"AST-{DateTime.UtcNow:yyyy}-{count + 1:D4}",
            Name = dto.Name, Category = dto.Category, Description = dto.Description,
            PurchaseDate = dto.PurchaseDate, PurchaseCost = dto.PurchaseCost,
            CurrentValue = dto.PurchaseCost,
            DepreciationMethod = method, UsefulLifeYears = dto.UsefulLifeYears,
            SalvageValue = dto.SalvageValue, Location = dto.Location,
            Vendor = dto.Vendor, ChamberId = chamberId
        };

        _context.Set<FixedAsset>().Add(asset);
        await _context.SaveChangesAsync();
        return (await GetAssetByIdAsync(asset.Id))!;
    }

    public async Task<FixedAssetResponseDto> UpdateAssetAsync(Guid id, CreateFixedAssetDto dto)
    {
        var asset = await _context.Set<FixedAsset>().FindAsync(id)
            ?? throw new KeyNotFoundException("Asset not found");
        asset.Name = dto.Name; asset.Category = dto.Category;
        asset.Description = dto.Description; asset.PurchaseDate = dto.PurchaseDate;
        asset.PurchaseCost = dto.PurchaseCost; asset.UsefulLifeYears = dto.UsefulLifeYears;
        asset.SalvageValue = dto.SalvageValue; asset.Location = dto.Location;
        asset.Vendor = dto.Vendor;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetAssetByIdAsync(id))!;
    }

    public async Task DeleteAssetAsync(Guid id)
    {
        var asset = await _context.Set<FixedAsset>().FindAsync(id)
            ?? throw new KeyNotFoundException("Asset not found");
        asset.IsDeleted = true; asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<FixedAssetResponseDto>> GetAssetsAsync(Guid chamberId)
    {
        return await _context.Set<FixedAsset>()
            .Where(a => a.ChamberId == chamberId && !a.IsDeleted)
            .OrderByDescending(a => a.PurchaseDate)
            .Select(a => MapAsset(a))
            .ToListAsync();
    }

    public async Task<FixedAssetResponseDto?> GetAssetByIdAsync(Guid id)
    {
        var asset = await _context.Set<FixedAsset>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        return asset == null ? null : MapAsset(asset);
    }

    public async Task<FixedAssetResponseDto> DisposeAssetAsync(Guid id, DateTime disposalDate, string reason)
    {
        var asset = await _context.Set<FixedAsset>().FindAsync(id)
            ?? throw new KeyNotFoundException("Asset not found");
        asset.Status = AssetStatus.Disposed;
        asset.DisposalDate = disposalDate;
        asset.DisposalReason = reason;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetAssetByIdAsync(id))!;
    }

    private static FixedAssetResponseDto MapAsset(FixedAsset a) => new()
    {
        Id = a.Id, AssetCode = a.AssetCode, Name = a.Name,
        Category = a.Category, Description = a.Description,
        PurchaseDate = a.PurchaseDate, PurchaseCost = a.PurchaseCost,
        CurrentValue = a.CurrentValue,
        DepreciationMethod = a.DepreciationMethod.ToString(),
        UsefulLifeYears = a.UsefulLifeYears, SalvageValue = a.SalvageValue,
        AccumulatedDepreciation = a.AccumulatedDepreciation,
        Location = a.Location, Vendor = a.Vendor,
        Status = a.Status.ToString(), DisposalDate = a.DisposalDate,
        CreatedAt = a.CreatedAt
    };
}
