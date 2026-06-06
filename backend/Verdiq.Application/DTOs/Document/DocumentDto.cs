namespace Verdiq.Application.DTOs.Document;

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FolderPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public Guid CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int VersionCount { get; set; }
    public string? Tags { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public bool IsFavorited { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int CommentCount { get; set; }
    public List<DocumentVersionDto> Versions { get; set; } = new();
    public List<DocumentShareDto> Shares { get; set; } = new();
}

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChangeNotes { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DocumentShareDto
{
    public Guid Id { get; set; }
    public Guid SharedWithUserId { get; set; }
    public string SharedWithUserName { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DocumentCommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<DocumentCommentDto> Replies { get; set; } = new();
}

public class DocumentActivityDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DocumentTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long FileSize { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDocumentTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = true;
}

public class UpdateDocumentDto
{
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public string? FolderPath { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ApprovalStatus { get; set; }
}

public class ShareDocumentDto
{
    public Guid SharedWithUserId { get; set; }
    public string Permissions { get; set; } = "View";
}

public class AddDocumentCommentDto
{
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
