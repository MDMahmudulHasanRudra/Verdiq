using Verdiq.Application.DTOs.ClientPortal;

namespace Verdiq.Application.Interfaces;

public interface IClientPortalService
{
    Task<ClientDashboardDto> GetDashboardAsync(Guid clientId);
    Task<ClientProfileDto> GetProfileAsync(Guid clientId);
    Task<IEnumerable<ClientCaseSummaryDto>> GetCasesAsync(Guid clientId);
    Task<ClientCaseDetailDto?> GetCaseDetailAsync(Guid caseId, Guid clientId);
    Task<IEnumerable<ClientHearingDto>> GetUpcomingHearingsAsync(Guid clientId);
    Task<IEnumerable<ClientDocumentDto>> GetDocumentsAsync(Guid clientId);
    Task<ClientDocumentDto?> GetDocumentByIdAsync(Guid documentId, Guid clientId);
    Task<IEnumerable<ClientInvoiceDto>> GetInvoicesAsync(Guid clientId);
    Task<IEnumerable<ClientTaskDto>> GetTasksAsync(Guid clientId);
}
