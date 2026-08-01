using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class CasePhotoService : ICasePhotoService
{
    private readonly AppDbContext _context;
    private readonly ICloudStorageService _cloudStorage;

    public CasePhotoService(AppDbContext context, ICloudStorageService cloudStorage)
    {
        _context = context;
        _cloudStorage = cloudStorage;
    }

    public async Task<IEnumerable<CasePhotoDto>> GetByCaseIdAsync(Guid caseId)
    {
        var photos = await _context.CasePhotos
            .Include(p => p.UploadedBy)
            .Where(p => p.CaseId == caseId && !p.IsDeleted)
            .OrderByDescending(p => p.CapturedAt)
            .ToListAsync();

        return photos.Select(MapToDto);
    }

    public async Task<(bool Success, string Message, CasePhotoDto? Data)> UploadAsync(Guid caseId, Guid userId, Stream fileStream, string fileName, string contentType, string? caption)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null || caseEntity.IsDeleted)
            return (false, "Case not found", null);

        var key = $"cases/{caseId}/photos/{Guid.NewGuid():N}_{fileName}";
        var storageKey = await _cloudStorage.UploadAsync(key, fileStream, contentType);

        var photo = new CasePhoto
        {
            CaseId = caseId,
            FileName = Path.GetFileName(storageKey),
            OriginalFileName = fileName,
            StorageKey = storageKey,
            ContentType = contentType,
            FileSize = fileStream.Length,
            Caption = caption,
            CapturedAt = DateTime.UtcNow,
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CasePhotos.Add(photo);
        await _context.SaveChangesAsync();

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            ActivityType = Domain.Enums.ActivityType.Document,
            Description = $"Photo added: {fileName}",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(photo.Id);
        return (true, "Photo uploaded successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid photoId)
    {
        var photo = await _context.CasePhotos.FindAsync(photoId);
        if (photo == null || photo.IsDeleted)
            return (false, "Photo not found");

        if (!string.IsNullOrWhiteSpace(photo.StorageKey))
            await _cloudStorage.DeleteAsync(photo.StorageKey);

        photo.IsDeleted = true;
        photo.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Photo deleted successfully");
    }

    public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAsync(Guid photoId)
    {
        var photo = await _context.CasePhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && !p.IsDeleted);
        if (photo == null || string.IsNullOrWhiteSpace(photo.StorageKey))
            return (null, null, null);

        var stream = await _cloudStorage.DownloadAsync(photo.StorageKey);
        return (stream, photo.ContentType, photo.OriginalFileName);
    }

    private async Task<CasePhotoDto?> GetByIdAsync(Guid photoId)
    {
        var photo = await _context.CasePhotos
            .Include(p => p.UploadedBy)
            .FirstOrDefaultAsync(p => p.Id == photoId && !p.IsDeleted);

        return photo == null ? null : MapToDto(photo);
    }

    private static CasePhotoDto MapToDto(CasePhoto p) => new()
    {
        Id = p.Id,
        CaseId = p.CaseId,
        FileName = p.FileName,
        OriginalFileName = p.OriginalFileName,
        ContentType = p.ContentType,
        FileSize = p.FileSize,
        Caption = p.Caption,
        CapturedAt = p.CapturedAt,
        UploadedByName = p.UploadedBy?.FullName,
        CreatedAt = p.CreatedAt
    };
}
