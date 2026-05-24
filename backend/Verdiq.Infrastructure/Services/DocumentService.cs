using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Document;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
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

    public async Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(
        Guid? caseId = null, string? category = null, string? tag = null)
    {
        var query = _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Tags)
            .Include(d => d.Versions)
            .Where(d => !d.IsDeleted);

        if (caseId.HasValue)
            query = query.Where(d => d.CaseId == caseId.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(d => d.Tags.Any(t => t.TagName == tag));

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<DocumentResponseDto> GetDocumentByIdAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Tags)
            .Include(d => d.Versions).ThenInclude(v => v.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        return MapToDto(document);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsByCaseIdAsync(Guid caseId)
    {
        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Tags)
            .Include(d => d.Versions)
            .Where(d => d.CaseId == caseId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<DocumentResponseDto> UploadDocumentAsync(Stream fileStream, string fileName,
        string contentType, long fileSize, string documentType, string category,
        Guid caseId, Guid uploadedById, List<string>? tags = null)
    {
        var storageResult = await _cloudStorage.UploadAsync(fileStream, fileName, contentType);

        var document = new Document
        {
            FileName = Path.GetFileName(storageResult.StorageKey),
            OriginalFileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = storageResult.FilePath,
            StorageProvider = storageResult.StorageProvider,
            StorageKey = storageResult.StorageKey,
            DocumentType = documentType,
            Category = category,
            Status = DocumentStatus.Draft,
            CurrentVersion = 1,
            CaseId = caseId,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        if (tags?.Count > 0)
        {
            document.Tags = tags.Select(t => new DocumentTag
            {
                TagName = t.Trim().ToLower(),
                DocumentId = document.Id,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        var version = new DocumentVersion
        {
            VersionNumber = 1,
            FileName = document.FileName,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            FilePath = document.FilePath,
            StorageProvider = document.StorageProvider,
            StorageKey = document.StorageKey,
            Status = DocumentStatus.Draft,
            ChangeNotes = "Initial upload",
            DocumentId = document.Id,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Documents.AddAsync(document);
        _context.DocumentVersions.Add(version);
        await _unitOfWork.CompleteAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<DocumentResponseDto> UploadNewVersionAsync(Guid documentId, Stream fileStream,
        string fileName, string contentType, long fileSize, Guid uploadedById, string? changeNotes = null)
    {
        var document = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Tags)
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        var storageResult = await _cloudStorage.UploadAsync(fileStream, fileName, contentType);

        var newVersionNumber = document.CurrentVersion + 1;

        var version = new DocumentVersion
        {
            VersionNumber = newVersionNumber,
            FileName = Path.GetFileName(storageResult.StorageKey),
            OriginalFileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = storageResult.FilePath,
            StorageProvider = storageResult.StorageProvider,
            StorageKey = storageResult.StorageKey,
            Status = document.Status,
            ChangeNotes = changeNotes ?? $"Version {newVersionNumber}",
            DocumentId = document.Id,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        document.FileName = version.FileName;
        document.OriginalFileName = version.OriginalFileName;
        document.ContentType = version.ContentType;
        document.FileSize = version.FileSize;
        document.FilePath = version.FilePath;
        document.StorageProvider = version.StorageProvider;
        document.StorageKey = version.StorageKey;
        document.CurrentVersion = newVersionNumber;
        document.UpdatedAt = DateTime.UtcNow;

        _context.DocumentVersions.Add(version);
        await _unitOfWork.CompleteAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<List<DocumentVersionDto>> GetVersionHistoryAsync(Guid documentId)
    {
        var versions = await _context.DocumentVersions
            .Include(v => v.UploadedBy)
            .Where(v => v.DocumentId == documentId && !v.IsDeleted)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(v => new DocumentVersionDto
        {
            Id = v.Id,
            VersionNumber = v.VersionNumber,
            FileName = v.FileName,
            OriginalFileName = v.OriginalFileName,
            ContentType = v.ContentType,
            FileSize = v.FileSize,
            Status = v.Status.ToString(),
            ChangeNotes = v.ChangeNotes,
            UploadedByName = v.UploadedBy.FullName,
            CreatedAt = v.CreatedAt
        }).ToList();
    }

    public async Task<DocumentResponseDto> RestoreVersionAsync(Guid documentId, Guid versionId, Guid userId)
    {
        var document = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Tags)
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        var version = await _context.DocumentVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.DocumentId == documentId && !v.IsDeleted);

        if (version == null)
            throw new KeyNotFoundException("Version not found");

        var newVersionNumber = document.CurrentVersion + 1;

        var restoredVersion = new DocumentVersion
        {
            VersionNumber = newVersionNumber,
            FileName = version.FileName,
            OriginalFileName = version.OriginalFileName,
            ContentType = version.ContentType,
            FileSize = version.FileSize,
            FilePath = version.FilePath,
            StorageProvider = version.StorageProvider,
            StorageKey = version.StorageKey,
            Status = document.Status,
            ChangeNotes = $"Restored from version {version.VersionNumber}",
            DocumentId = document.Id,
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        document.FileName = version.FileName;
        document.OriginalFileName = version.OriginalFileName;
        document.ContentType = version.ContentType;
        document.FileSize = version.FileSize;
        document.FilePath = version.FilePath;
        document.StorageProvider = version.StorageProvider;
        document.StorageKey = version.StorageKey;
        document.CurrentVersion = newVersionNumber;
        document.UpdatedAt = DateTime.UtcNow;

        _context.DocumentVersions.Add(restoredVersion);
        await _unitOfWork.CompleteAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<string> GenerateSignedUrlAsync(Guid documentId)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        if (!string.IsNullOrEmpty(document.StorageKey) && document.StorageProvider != "Local")
        {
            return await _cloudStorage.GenerateSignedUrlAsync(document.StorageKey);
        }

        return string.Empty;
    }

    public async Task AddTagAsync(Guid documentId, string tagName)
    {
        var document = await _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        var normalizedTag = tagName.Trim().ToLower();

        if (document.Tags.Any(t => t.TagName == normalizedTag))
            return;

        _context.DocumentTags.Add(new DocumentTag
        {
            TagName = normalizedTag,
            DocumentId = documentId,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task RemoveTagAsync(Guid documentId, string tagName)
    {
        var tag = await _context.DocumentTags
            .FirstOrDefaultAsync(t => t.DocumentId == documentId && t.TagName == tagName.Trim().ToLower());

        if (tag == null)
            throw new KeyNotFoundException("Tag not found");

        _context.DocumentTags.Remove(tag);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<List<DocumentTagDto>> GetTagsAsync(Guid documentId)
    {
        var tags = await _context.DocumentTags
            .Where(t => t.DocumentId == documentId)
            .ToListAsync();

        return tags.Select(t => new DocumentTagDto
        {
            Id = t.Id,
            TagName = t.TagName
        }).ToList();
    }

    public async Task<BulkOperationResult> BulkDeleteAsync(List<Guid> ids)
    {
        var result = new BulkOperationResult();

        foreach (var id in ids)
        {
            try
            {
                await DeleteDocumentAsync(id);
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Document {id}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
            throw new KeyNotFoundException("Document not found");

        foreach (var tag in document.Tags.ToList())
            _context.DocumentTags.Remove(tag);

        if (!string.IsNullOrEmpty(document.StorageKey))
            await _cloudStorage.DeleteAsync(document.StorageKey);

        await _unitOfWork.Documents.DeleteAsync(document);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        var stream = await _cloudStorage.DownloadAsync(document.StorageKey ?? document.FileName);
        return (stream, document.ContentType, document.OriginalFileName);
    }

    private DocumentResponseDto MapToDto(Document d)
    {
        return new DocumentResponseDto
        {
            Id = d.Id,
            FileName = d.FileName,
            OriginalFileName = d.OriginalFileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            DocumentType = d.DocumentType,
            Category = d.Category,
            Status = d.Status.ToString(),
            CaseId = d.CaseId,
            CaseTitle = d.Case?.Title ?? string.Empty,
            UploadedByName = d.UploadedBy?.FullName ?? string.Empty,
            CreatedAt = d.CreatedAt,
            StorageProvider = d.StorageProvider,
            CurrentVersion = d.CurrentVersion,
            VersionCount = d.Versions?.Count ?? 0,
            Tags = d.Tags?.Select(t => t.TagName).ToList() ?? new List<string>(),
            Versions = d.Versions?
                .Where(v => !v.IsDeleted)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DocumentVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    FileName = v.FileName,
                    OriginalFileName = v.OriginalFileName,
                    ContentType = v.ContentType,
                    FileSize = v.FileSize,
                    Status = v.Status.ToString(),
                    ChangeNotes = v.ChangeNotes,
                    UploadedByName = v.UploadedBy?.FullName ?? string.Empty,
                    CreatedAt = v.CreatedAt
                }).ToList() ?? new List<DocumentVersionDto>()
        };
    }
}
