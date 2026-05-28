using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.ClientPortal;
using Verdiq.Application.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ClientPortalService : IClientPortalService
{
    private readonly AppDbContext _context;

    public ClientPortalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClientDashboardDto> GetDashboardAsync(Guid clientId)
    {
        var client = await _context.Clients
            .Include(c => c.Chamber)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null) return new ClientDashboardDto();

        var clientCaseIds = await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Select(cc => cc.CaseId)
            .ToListAsync();

        var userId = client.UserId;

        var activeCases = await _context.Cases
            .Where(c => clientCaseIds.Contains(c.Id) && c.Status == Domain.Enums.CaseStatus.Active)
            .CountAsync();

        var upcomingHearings = await _context.Hearings
            .Where(h => clientCaseIds.Contains(h.CaseId) && h.HearingDate > DateTime.UtcNow && h.Status == Domain.Enums.HearingStatus.Scheduled)
            .CountAsync();

        var invoices = await _context.Invoices
            .Where(i => i.ClientId == clientId)
            .ToListAsync();

        var pendingInvoices = invoices.Count(i => i.Status == Domain.Enums.PaymentStatus.Pending);
        var outstandingBalance = invoices
            .Where(i => i.Status != Domain.Enums.PaymentStatus.Completed)
            .Sum(i => i.Amount);

        var sharedDocuments = await _context.Documents
            .Where(d => d.SharedWithClientId == clientId || (d.Visibility == "SharedWithClient" && clientCaseIds.Contains(d.CaseId)))
            .CountAsync();

        var unreadMessages = userId.HasValue
            ? await _context.Messages.Where(m => m.ReceiverId == userId.Value && !m.IsRead).CountAsync()
            : 0;

        var pendingTasks = userId.HasValue
            ? await _context.Tasks.Where(t => t.AssignedTo == userId.Value && t.Status == Domain.Enums.TaskStatus.Pending).CountAsync()
            : 0;

        var recentCases = await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Include(cc => cc.Case).ThenInclude(c => c.AssignedLawyer)
            .OrderByDescending(cc => cc.Case.CreatedAt)
            .Take(5)
            .Select(cc => new ClientCaseSummaryDto
            {
                Id = cc.CaseId,
                CaseNumber = cc.Case.CaseNumber,
                Title = cc.Case.Title,
                CaseType = cc.Case.CaseType,
                Status = cc.Case.Status.ToString(),
                AssignedLawyerName = cc.Case.AssignedLawyer.FullName,
                NextHearingDate = _context.Hearings
                    .Where(h => h.CaseId == cc.CaseId && h.HearingDate > DateTime.UtcNow)
                    .OrderBy(h => h.HearingDate)
                    .Select(h => (DateTime?)h.HearingDate)
                    .FirstOrDefault(),
                DocumentsCount = _context.Documents.Count(d => d.CaseId == cc.CaseId),
                CreatedAt = cc.Case.CreatedAt
            })
            .ToListAsync();

        var upcomingHearingList = await _context.Hearings
            .Where(h => clientCaseIds.Contains(h.CaseId) && h.HearingDate > DateTime.UtcNow && h.Status == Domain.Enums.HearingStatus.Scheduled)
            .Include(h => h.Case)
            .OrderBy(h => h.HearingDate)
            .Take(5)
            .Select(h => new ClientHearingDto
            {
                Id = h.Id,
                CaseId = h.CaseId,
                CaseTitle = h.Case.Title,
                CaseNumber = h.Case.CaseNumber,
                HearingDate = h.HearingDate,
                Courtroom = h.Courtroom,
                JudgeName = h.JudgeName,
                Status = h.Status.ToString()
            })
            .ToListAsync();

        var recentInvoices = await _context.Invoices
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new ClientInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Amount = i.Amount,
                PaidAmount = 0,
                Balance = i.Amount,
                Currency = i.Currency,
                Status = i.Status.ToString(),
                Description = i.Description,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt,
                CaseTitle = i.Case != null ? i.Case.Title : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return new ClientDashboardDto
        {
            ActiveCases = activeCases,
            UpcomingHearings = upcomingHearings,
            PendingInvoices = pendingInvoices,
            OutstandingBalance = outstandingBalance,
            SharedDocuments = sharedDocuments,
            UnreadMessages = unreadMessages,
            PendingTasks = pendingTasks,
            RecentCases = recentCases.ToArray(),
            UpcomingHearingList = upcomingHearingList.ToArray(),
            RecentInvoices = recentInvoices.ToArray()
        };
    }

    public async Task<ClientProfileDto> GetProfileAsync(Guid clientId)
    {
        var client = await _context.Clients
            .Include(c => c.Chamber)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null) return new ClientProfileDto();

        return new ClientProfileDto
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            CompanyName = client.CompanyName,
            ChamberId = client.ChamberId,
            ChamberName = client.Chamber.Name,
            ChamberLogo = client.Chamber.Logo
        };
    }

    public async Task<IEnumerable<ClientCaseSummaryDto>> GetCasesAsync(Guid clientId)
    {
        return await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Include(cc => cc.Case).ThenInclude(c => c.AssignedLawyer)
            .OrderByDescending(cc => cc.Case.CreatedAt)
            .Select(cc => new ClientCaseSummaryDto
            {
                Id = cc.CaseId,
                CaseNumber = cc.Case.CaseNumber,
                Title = cc.Case.Title,
                CaseType = cc.Case.CaseType,
                Status = cc.Case.Status.ToString(),
                AssignedLawyerName = cc.Case.AssignedLawyer.FullName,
                NextHearingDate = _context.Hearings
                    .Where(h => h.CaseId == cc.CaseId && h.HearingDate > DateTime.UtcNow)
                    .OrderBy(h => h.HearingDate)
                    .Select(h => (DateTime?)h.HearingDate)
                    .FirstOrDefault(),
                DocumentsCount = _context.Documents.Count(d => d.CaseId == cc.CaseId),
                CreatedAt = cc.Case.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ClientCaseDetailDto?> GetCaseDetailAsync(Guid caseId, Guid clientId)
    {
        var isLinked = await _context.ClientCases.AnyAsync(cc => cc.ClientId == clientId && cc.CaseId == caseId);
        if (!isLinked) return null;

        var caseEntity = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseEntity == null) return null;

        var hearings = await _context.Hearings
            .Where(h => h.CaseId == caseId)
            .OrderBy(h => h.HearingDate)
            .ToListAsync();

        var activities = await _context.CaseActivities
            .Where(a => a.CaseId == caseId && a.IsClientVisible)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var timeline = new List<ClientTimelineEntryDto>();

        foreach (var a in activities)
        {
            timeline.Add(new ClientTimelineEntryDto
            {
                Id = a.Id,
                Type = a.ActivityType.ToString(),
                Description = a.Description,
                Timestamp = a.CreatedAt
            });
        }

        foreach (var h in hearings)
        {
            timeline.Add(new ClientTimelineEntryDto
            {
                Id = h.Id,
                Type = "Hearing",
                Description = $"Hearing on {h.HearingDate:MMM dd, yyyy} - {(h.Result ?? h.Status.ToString())}",
                Timestamp = h.CreatedAt
            });
        }

        timeline = timeline.OrderByDescending(t => t.Timestamp).ToList();

        return new ClientCaseDetailDto
        {
            Id = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            Title = caseEntity.Title,
            CourtName = caseEntity.CourtName,
            CaseType = caseEntity.CaseType,
            Status = caseEntity.Status.ToString(),
            Opponent = caseEntity.Opponent,
            AssignedLawyerName = caseEntity.AssignedLawyer.FullName,
            AssignedLawyerPhone = caseEntity.AssignedLawyer.Phone,
            AssignedLawyerEmail = caseEntity.AssignedLawyer.Email,
            FilingDate = caseEntity.FilingDate,
            CreatedAt = caseEntity.CreatedAt,
            Timeline = timeline.ToArray()
        };
    }

    public async Task<IEnumerable<ClientHearingDto>> GetUpcomingHearingsAsync(Guid clientId)
    {
        var clientCaseIds = await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Select(cc => cc.CaseId)
            .ToListAsync();

        return await _context.Hearings
            .Where(h => clientCaseIds.Contains(h.CaseId) && h.HearingDate > DateTime.UtcNow)
            .Include(h => h.Case)
            .OrderBy(h => h.HearingDate)
            .Select(h => new ClientHearingDto
            {
                Id = h.Id,
                CaseId = h.CaseId,
                CaseTitle = h.Case.Title,
                CaseNumber = h.Case.CaseNumber,
                HearingDate = h.HearingDate,
                Courtroom = h.Courtroom,
                JudgeName = h.JudgeName,
                Status = h.Status.ToString(),
                Result = h.Result,
                NextHearingDate = h.NextHearingDate
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ClientDocumentDto>> GetDocumentsAsync(Guid clientId)
    {
        var clientCaseIds = await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Select(cc => cc.CaseId)
            .ToListAsync();

        return await _context.Documents
            .Where(d => d.SharedWithClientId == clientId ||
                        (d.Visibility == "SharedWithClient" && clientCaseIds.Contains(d.CaseId)))
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new ClientDocumentDto
            {
                Id = d.Id,
                FileName = d.OriginalFileName,
                FileType = d.FileType,
                FileSize = d.FileSize,
                Category = d.Category,
                FolderPath = d.FolderPath,
                CaseId = d.CaseId,
                CaseTitle = d.Case.Title,
                UploadedByName = d.UploadedBy.FullName,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ClientDocumentDto?> GetDocumentByIdAsync(Guid documentId, Guid clientId)
    {
        var clientCaseIds = await _context.ClientCases
            .Where(cc => cc.ClientId == clientId)
            .Select(cc => cc.CaseId)
            .ToListAsync();

        return await _context.Documents
            .Where(d => d.Id == documentId &&
                        (d.SharedWithClientId == clientId ||
                         (d.Visibility == "SharedWithClient" && clientCaseIds.Contains(d.CaseId))))
            .Include(d => d.Case)
            .Include(d => d.UploadedBy)
            .Select(d => new ClientDocumentDto
            {
                Id = d.Id,
                FileName = d.OriginalFileName,
                FileType = d.FileType,
                FileSize = d.FileSize,
                Category = d.Category,
                FolderPath = d.FolderPath,
                CaseId = d.CaseId,
                CaseTitle = d.Case.Title,
                UploadedByName = d.UploadedBy.FullName,
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ClientInvoiceDto>> GetInvoicesAsync(Guid clientId)
    {
        return await _context.Invoices
            .Where(i => i.ClientId == clientId)
            .Include(i => i.Case)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new ClientInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Amount = i.Amount,
                PaidAmount = 0,
                Balance = i.Amount,
                Currency = i.Currency,
                Status = i.Status.ToString(),
                Description = i.Description,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt,
                CaseTitle = i.Case != null ? i.Case.Title : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ClientTaskDto>> GetTasksAsync(Guid clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client?.UserId == null) return Enumerable.Empty<ClientTaskDto>();

        return await _context.Tasks
            .Where(t => t.AssignedTo == client.UserId)
            .Include(t => t.Assigner)
            .Include(t => t.Case)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ClientTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.Status.ToString(),
                AssignedByName = t.Assigner.FullName,
                CaseTitle = t.Case != null ? t.Case.Title : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }
}
