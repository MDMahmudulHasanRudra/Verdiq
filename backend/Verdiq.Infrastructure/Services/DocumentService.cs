using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Document;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly ICloudStorageService _cloudStorage;

    public DocumentService(IUnitOfWork unitOfWork, AppDbContext context, ICloudStorageService cloudStorage)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cloudStorage = cloudStorage;
    }

    public async Task<(bool Success, string Message, DocumentResponseDto? Data)> UploadAsync(
        Guid caseId, Guid userId, string category, string? folderPath, Stream fileStream, string fileName, string contentType)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null || caseEntity.IsDeleted)
            return (false, "Case not found", null);

        var key = $"cases/{caseId}/{category}/{Guid.NewGuid():N}_{fileName}";
        var storageKey = await _cloudStorage.UploadAsync(key, fileStream, contentType);

        var document = new Document
        {
            FileName = Path.GetFileName(storageKey),
            OriginalFileName = fileName,
            FilePath = storageKey,
            FileType = contentType,
            FileSize = fileStream.Length,
            Category = category,
            FolderPath = folderPath,
            Status = Domain.Enums.DocumentStatus.Draft,
            Version = 1,
            CaseId = caseId,
            UploadedById = userId,
            StorageProvider = "Local",
            StorageKey = storageKey,
            CreatedAt = DateTime.UtcNow
        };

        var version = new DocumentVersion
        {
            VersionNumber = 1,
            FileName = document.FileName,
            OriginalFileName = document.OriginalFileName,
            FilePath = document.FilePath,
            FileType = document.FileType,
            FileSize = document.FileSize,
            StorageProvider = document.StorageProvider,
            StorageKey = document.StorageKey,
            Status = Domain.Enums.DocumentStatus.Draft,
            ChangeNotes = "Initial upload",
            DocumentId = document.Id,
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Document>().AddAsync(document);
        _context.DocumentVersions.Add(version);
        await _unitOfWork.CompleteAsync();

        var result = await GetByIdAsync(document.Id);
        return (true, "Document uploaded successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            return (false, "Document not found");

        if (!string.IsNullOrEmpty(document.StorageKey))
            await _cloudStorage.DeleteAsync(document.StorageKey);

        await _unitOfWork.Repository<Document>().DeleteAsync(document);
        await _unitOfWork.CompleteAsync();

        return (true, "Document deleted successfully");
    }

    public async Task<DocumentResponseDto?> GetByIdAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return document == null ? null : MapToDto(document);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetByCaseIdAsync(Guid caseId)
    {
        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Where(d => d.CaseId == caseId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            return (null, null, null);

        var stream = await _cloudStorage.DownloadAsync(document.StorageKey ?? document.FileName);
        return (stream, document.FileType, document.OriginalFileName);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetAllAsync(Guid chamberId, string? category = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Where(d => !d.IsDeleted && d.Case.ChamberId == chamberId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    private static DocumentResponseDto MapToDto(Document d)
    {
        return new DocumentResponseDto
        {
            Id = d.Id,
            FileName = d.FileName,
            OriginalFileName = d.OriginalFileName,
            FileType = d.FileType,
            FileSize = d.FileSize,
            Category = d.Category,
            FolderPath = d.FolderPath,
            Status = d.Status.ToString(),
            Version = d.Version,
            CaseId = d.CaseId,
            CaseTitle = d.Case?.Title ?? string.Empty,
            UploadedByName = d.UploadedBy?.FullName ?? string.Empty,
            CreatedAt = d.CreatedAt,
            VersionCount = d.Versions?.Count ?? 0,
            Versions = d.Versions?
                .Where(v => !v.IsDeleted)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DocumentVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    FileName = v.FileName,
                    OriginalFileName = v.OriginalFileName,
                    FileType = v.FileType,
                    FileSize = v.FileSize,
                    Status = v.Status.ToString(),
                    ChangeNotes = v.ChangeNotes,
                    UploadedByName = v.UploadedBy?.FullName ?? string.Empty,
                    CreatedAt = v.CreatedAt
                }).ToList() ?? new List<DocumentVersionDto>()
        };
    }
}
