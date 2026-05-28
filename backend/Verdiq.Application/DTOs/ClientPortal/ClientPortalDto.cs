namespace Verdiq.Application.DTOs.ClientPortal;

public class ClientRegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
}

public class ClientDashboardDto
{
    public int ActiveCases { get; set; }
    public int UpcomingHearings { get; set; }
    public int PendingInvoices { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int SharedDocuments { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingTasks { get; set; }
    public ClientCaseSummaryDto[] RecentCases { get; set; } = Array.Empty<ClientCaseSummaryDto>();
    public ClientHearingDto[] UpcomingHearingList { get; set; } = Array.Empty<ClientHearingDto>();
    public ClientInvoiceDto[] RecentInvoices { get; set; } = Array.Empty<ClientInvoiceDto>();
}

public class ClientProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CompanyName { get; set; }
    public Guid ChamberId { get; set; }
    public string ChamberName { get; set; } = string.Empty;
    public string? ChamberLogo { get; set; }
}

public class ClientCaseSummaryDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedLawyerName { get; set; } = string.Empty;
    public DateTime? NextHearingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DocumentsCount { get; set; }
}

public class ClientCaseDetailDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Opponent { get; set; }
    public string AssignedLawyerName { get; set; } = string.Empty;
    public string? AssignedLawyerPhone { get; set; }
    public string? AssignedLawyerEmail { get; set; }
    public DateTime FilingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientTimelineEntryDto[] Timeline { get; set; } = Array.Empty<ClientTimelineEntryDto>();
}

public class ClientTimelineEntryDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ClientHearingDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public DateTime HearingDate { get; set; }
    public string? Courtroom { get; set; }
    public string? JudgeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTime? NextHearingDate { get; set; }
}

public class ClientDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FolderPath { get; set; }
    public Guid CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ClientInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? CaseTitle { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClientTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedByName { get; set; }
    public string? CaseTitle { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? CaseId { get; set; }
}

public class MarkMessageReadDto
{
    public Guid MessageId { get; set; }
}
