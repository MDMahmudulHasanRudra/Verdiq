using Verdiq.Application.DTOs.Audit;

namespace Verdiq.Application.Interfaces;

public interface IAuditService
{
    Task<AuditSummaryDto> GetSummaryAsync(Guid chamberId);
    Task<List<AuditLogResponseDto>> GetLogsAsync(Guid chamberId, string? entity = null, string? action = null, int page = 1, int pageSize = 50);
}
