using Verdiq.Application.DTOs.Audit;

namespace Verdiq.Application.Interfaces;

public interface IAuditService
{
    Task<AuditSummaryDto> GetSummaryAsync(Guid chamberId);
    Task<(List<AuditLogResponseDto> Items, int TotalCount)> GetLogsAsync(Guid chamberId, AuditLogFilterDto filter);
}
