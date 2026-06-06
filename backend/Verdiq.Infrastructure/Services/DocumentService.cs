using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Document;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

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

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = document.Id,
            UserId = userId,
            Action = "Uploaded",
            Details = $"Uploaded {fileName}",
            CreatedAt = DateTime.UtcNow
        });

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
            .Include(d => d.Shares).ThenInclude(s => s.SharedWithUser)
            .Include(d => d.Comments)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return document == null ? null : await MapToDtoAsync(document, null);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetByCaseIdAsync(Guid caseId)
    {
        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Include(d => d.Shares).ThenInclude(s => s.SharedWithUser)
            .Include(d => d.Comments)
            .Where(d => d.CaseId == caseId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return await MapToDtosAsync(documents, null);
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
            .Include(d => d.Shares).ThenInclude(s => s.SharedWithUser)
            .Include(d => d.Comments)
            .Where(d => !d.IsDeleted && d.Case.ChamberId == chamberId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToDtosAsync(documents, null);
    }

    public async Task<DocumentResponseDto> UpdateAsync(Guid id, UpdateDocumentDto dto, Guid userId)
    {
        var document = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Include(d => d.Shares).ThenInclude(s => s.SharedWithUser)
            .Include(d => d.Comments)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException("Document not found");

        if (dto.Description != null) document.Description = dto.Description;
        if (dto.Tags != null) document.Tags = dto.Tags;
        if (dto.Category != null) document.Category = dto.Category;
        if (dto.FolderPath != null) document.FolderPath = dto.FolderPath;
        if (dto.ExpiryDate.HasValue) document.ExpiryDate = dto.ExpiryDate;
        if (dto.ApprovalStatus != null)
        {
            document.ApprovalStatus = dto.ApprovalStatus;
            if (dto.ApprovalStatus == "Approved")
            {
                document.ApprovedById = userId;
                document.ApprovedAt = DateTime.UtcNow;
            }
        }

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await MapToDtoAsync(document, null))!;
    }

    public async Task ToggleFavoriteAsync(Guid documentId, Guid userId)
    {
        var existing = await _context.DocumentFavorites
            .FirstOrDefaultAsync(f => f.DocumentId == documentId && f.UserId == userId);

        if (existing != null)
        {
            _context.DocumentFavorites.Remove(existing);
        }
        else
        {
            _context.DocumentFavorites.Add(new DocumentFavorite
            {
                DocumentId = documentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<DocumentShareDto> ShareAsync(Guid documentId, ShareDocumentDto dto, Guid sharedById)
    {
        var document = await _context.Documents.FindAsync(documentId)
            ?? throw new KeyNotFoundException("Document not found");

        var share = new DocumentShare
        {
            DocumentId = documentId,
            SharedWithUserId = dto.SharedWithUserId,
            Permissions = dto.Permissions,
            SharedById = sharedById,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentShares.Add(share);

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = documentId,
            UserId = sharedById,
            Action = "Shared",
            Details = $"Shared with user {dto.SharedWithUserId}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var sharedUser = await _context.Users.FindAsync(dto.SharedWithUserId);
        return new DocumentShareDto
        {
            Id = share.Id,
            SharedWithUserId = share.SharedWithUserId,
            SharedWithUserName = sharedUser?.FullName ?? "Unknown",
            Permissions = share.Permissions,
            CreatedAt = share.CreatedAt
        };
    }

    public async Task RemoveShareAsync(Guid shareId)
    {
        var share = await _context.DocumentShares.FindAsync(shareId)
            ?? throw new KeyNotFoundException("Share not found");
        _context.DocumentShares.Remove(share);
        await _context.SaveChangesAsync();
    }

    public async Task<DocumentCommentDto> AddCommentAsync(Guid documentId, AddDocumentCommentDto dto, Guid userId)
    {
        var comment = new DocumentComment
        {
            DocumentId = documentId,
            UserId = userId,
            Content = dto.Content,
            ParentCommentId = dto.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentComments.Add(comment);

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = documentId,
            UserId = userId,
            Action = "Commented",
            Details = "Added a comment",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return new DocumentCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserId = userId,
            UserName = user?.FullName ?? "Unknown",
            CreatedAt = comment.CreatedAt,
            ParentCommentId = comment.ParentCommentId
        };
    }

    public async Task<IEnumerable<DocumentCommentDto>> GetCommentsAsync(Guid documentId)
    {
        var comments = await _context.DocumentComments
            .Include(c => c.User)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .Where(c => c.DocumentId == documentId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new DocumentCommentDto
        {
            Id = c.Id,
            Content = c.Content,
            UserId = c.UserId,
            UserName = c.User.FullName,
            UserAvatar = c.User.AvatarUrl,
            CreatedAt = c.CreatedAt,
            ParentCommentId = c.ParentCommentId,
            Replies = c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt).Select(r => new DocumentCommentDto
            {
                Id = r.Id,
                Content = r.Content,
                UserId = r.UserId,
                UserName = r.User.FullName,
                UserAvatar = r.User.AvatarUrl,
                CreatedAt = r.CreatedAt,
                ParentCommentId = r.ParentCommentId
            }).ToList()
        });
    }

    public async Task<IEnumerable<DocumentActivityDto>> GetActivityAsync(Guid documentId)
    {
        return await _context.DocumentActivities
            .Include(a => a.User)
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new DocumentActivityDto
            {
                Id = a.Id,
                Action = a.Action,
                Details = a.Details,
                UserId = a.UserId,
                UserName = a.User.FullName,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentTemplateDto>> GetTemplatesAsync(Guid chamberId, string? category = null)
    {
        var query = _context.DocumentTemplates
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category);

        return await query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new DocumentTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                FileType = t.FileType,
                FileSize = t.FileSize,
                Tags = t.Tags,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DocumentResponseDto> CreateFromTemplateAsync(Guid templateId, Guid caseId, Guid userId)
    {
        var template = await _context.DocumentTemplates.FindAsync(templateId)
            ?? throw new KeyNotFoundException("Template not found");

        var caseEntity = await _context.Cases.FindAsync(caseId)
            ?? throw new KeyNotFoundException("Case not found");

        var document = new Document
        {
            FileName = template.FileName ?? template.Name,
            OriginalFileName = template.Name,
            FilePath = template.FilePath ?? "",
            FileType = template.FileType ?? "application/octet-stream",
            FileSize = template.FileSize,
            Category = template.Category,
            Status = Domain.Enums.DocumentStatus.Draft,
            Version = 1,
            CaseId = caseId,
            UploadedById = userId,
            StorageProvider = "Local",
            StorageKey = template.StorageKey,
            Tags = template.Tags,
            Description = template.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = document.Id,
            UserId = userId,
            Action = "Created from template",
            Details = $"Created from template '{template.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(document.Id))!;
    }

    public async Task<DocumentTemplateDto> CreateTemplateAsync(CreateDocumentTemplateDto dto, Guid chamberId, Guid userId, Stream? fileStream = null, string? fileName = null, string? contentType = null)
    {
        var template = new DocumentTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Tags = dto.Tags,
            IsPublic = dto.IsPublic,
            ChamberId = chamberId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (fileStream != null && fileName != null)
        {
            var key = $"templates/{chamberId}/{Guid.NewGuid():N}_{fileName}";
            var storageKey = await _cloudStorage.UploadAsync(key, fileStream, contentType ?? "application/octet-stream");
            template.FilePath = storageKey;
            template.StorageKey = storageKey;
            template.FileType = contentType;
            template.FileSize = fileStream.Length;
            template.FileName = Path.GetFileName(storageKey);
        }

        _context.DocumentTemplates.Add(template);
        await _context.SaveChangesAsync();

        return new DocumentTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            FileType = template.FileType,
            FileSize = template.FileSize,
            Tags = template.Tags,
            CreatedAt = template.CreatedAt
        };
    }

    public async Task<IEnumerable<DocumentResponseDto>> SearchAsync(Guid chamberId, string query, int page = 1, int pageSize = 20)
    {
        var lowerQuery = query.ToLowerInvariant();
        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Include(d => d.Shares).ThenInclude(s => s.SharedWithUser)
            .Include(d => d.Comments)
            .Where(d => !d.IsDeleted && d.Case.ChamberId == chamberId
                && (d.OriginalFileName.ToLower().Contains(lowerQuery)
                    || d.FileName.ToLower().Contains(lowerQuery)
                    || (d.Tags != null && d.Tags.ToLower().Contains(lowerQuery))
                    || (d.Description != null && d.Description.ToLower().Contains(lowerQuery))
                    || d.Category.ToLower().Contains(lowerQuery)))
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToDtosAsync(documents, null);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetRecentAsync(Guid chamberId, int count = 10)
    {
        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Where(d => !d.IsDeleted && d.Case.ChamberId == chamberId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .ToListAsync();

        return await MapToDtosAsync(documents, null);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetFavoritesAsync(Guid userId)
    {
        var documentIds = await _context.DocumentFavorites
            .Where(f => f.UserId == userId)
            .Select(f => f.DocumentId)
            .ToListAsync();

        var documents = await _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Include(d => d.Versions)
            .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return await MapToDtosAsync(documents, userId);
    }

    public async Task RecordViewAsync(Guid documentId, Guid userId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null) return;

        document.ViewCount++;
        document.UpdatedAt = DateTime.UtcNow;

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = documentId,
            UserId = userId,
            Action = "Viewed",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task RecordDownloadAsync(Guid documentId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null) return;

        document.DownloadCount++;
        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> BulkDeleteAsync(List<Guid> ids)
    {
        var documents = await _context.Documents
            .Where(d => ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync();

        foreach (var doc in documents)
        {
            doc.IsDeleted = true;
            doc.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, $"{documents.Count} document(s) deleted");
    }

    public async Task<(bool Success, string Message)> BulkUpdateStatusAsync(List<Guid> ids, string status)
    {
        if (!Enum.TryParse<Domain.Enums.DocumentStatus>(status, true, out var docStatus))
            return (false, "Invalid status");

        var documents = await _context.Documents
            .Where(d => ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync();

        foreach (var doc in documents)
        {
            doc.Status = docStatus;
            doc.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, $"{documents.Count} document(s) updated to {status}");
    }

    public async Task<(bool Success, string Message)> BulkUpdateCategoryAsync(List<Guid> ids, string category)
    {
        var documents = await _context.Documents
            .Where(d => ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync();

        foreach (var doc in documents)
        {
            doc.Category = category;
            doc.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, $"{documents.Count} document(s) updated to category '{category}'");
    }

    public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadTemplateAsync(Guid templateId)
    {
        var template = await _context.DocumentTemplates.FindAsync(templateId);
        if (template == null || template.IsDeleted)
            return (null, null, null);

        var stream = await _cloudStorage.DownloadAsync(template.StorageKey ?? template.FilePath ?? "");
        return (stream, template.FileType, template.Name);
    }

    private async Task<DocumentResponseDto?> MapToDtoAsync(Document? d, Guid? currentUserId)
    {
        if (d == null) return null;
        return (await MapToDtosAsync(new[] { d }, currentUserId)).FirstOrDefault();
    }

    private async Task<List<DocumentResponseDto>> MapToDtosAsync(IEnumerable<Document> documents, Guid? currentUserId)
    {
        var docList = documents.ToList();
        var docIds = docList.Select(d => d.Id).ToList();

        List<DocumentFavorite>? favorites = null;
        if (currentUserId.HasValue)
        {
            favorites = await _context.DocumentFavorites
                .Where(f => docIds.Contains(f.DocumentId) && f.UserId == currentUserId.Value)
                .ToListAsync();
        }

        return docList.Select(d => new DocumentResponseDto
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
            Tags = d.Tags,
            Description = d.Description,
            ExpiryDate = d.ExpiryDate,
            ViewCount = d.ViewCount,
            DownloadCount = d.DownloadCount,
            IsFavorited = favorites?.Any(f => f.DocumentId == d.Id) ?? false,
            ApprovalStatus = d.ApprovalStatus,
            ApprovedByName = d.ApprovedBy?.FullName,
            ApprovedAt = d.ApprovedAt,
            CommentCount = d.Comments?.Count ?? 0,
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
                }).ToList() ?? new List<DocumentVersionDto>(),
            Shares = d.Shares?
                .Where(s => !s.IsDeleted)
                .Select(s => new DocumentShareDto
                {
                    Id = s.Id,
                    SharedWithUserId = s.SharedWithUserId,
                    SharedWithUserName = s.SharedWithUser?.FullName ?? "Unknown",
                    Permissions = s.Permissions,
                    CreatedAt = s.CreatedAt
                }).ToList() ?? new List<DocumentShareDto>()
        }).ToList();
    }
}
