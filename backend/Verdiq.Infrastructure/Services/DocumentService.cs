using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly string _storagePath;

    public DocumentService(IUnitOfWork unitOfWork, AppDbContext context, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _storagePath = configuration["DocumentStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Documents");
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(Guid? caseId = null, string? category = null)
    {
        var query = _context.Documents
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Where(d => !d.IsDeleted);

        if (caseId.HasValue)
            query = query.Where(d => d.CaseId == caseId.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

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
            .Where(d => d.CaseId == caseId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<DocumentResponseDto> UploadDocumentAsync(Stream fileStream, string fileName,
        string contentType, long fileSize, string documentType, string category, Guid caseId, Guid uploadedById)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        var document = new Document
        {
            FileName = uniqueFileName,
            OriginalFileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            DocumentType = documentType,
            Category = category,
            Status = DocumentStatus.Draft,
            CaseId = caseId,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.CompleteAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        if (File.Exists(document.FilePath))
            File.Delete(document.FilePath);

        await _unitOfWork.Documents.DeleteAsync(document);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || document.IsDeleted)
            throw new KeyNotFoundException("Document not found");

        if (!File.Exists(document.FilePath))
            throw new FileNotFoundException("Document file not found on disk");

        var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
        return (stream, document.ContentType, document.OriginalFileName);
    }

    private static DocumentResponseDto MapToDto(Document d)
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
            CaseTitle = d.Case.Title,
            UploadedByName = d.UploadedBy.FullName,
            CreatedAt = d.CreatedAt
        };
    }
}
