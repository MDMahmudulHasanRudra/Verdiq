using Verdiq.Application.DTOs.Accounting;

namespace Verdiq.Application.Interfaces;

public interface IAccountingService
{
    Task<JournalResponseDto> CreateJournalAsync(CreateJournalDto dto, Guid userId, Guid chamberId);
    Task<JournalResponseDto> UpdateJournalAsync(Guid id, CreateJournalDto dto);
    Task DeleteJournalAsync(Guid id);
    Task<JournalResponseDto?> GetJournalByIdAsync(Guid id);
    Task<(List<JournalResponseDto> Items, int TotalCount)> GetJournalsAsync(Guid chamberId, int page = 1, int pageSize = 20, DateTime? from = null, DateTime? to = null, Guid? accountId = null);
    Task<AccountingDashboardDto> GetDashboardAsync(Guid chamberId);
    Task<ProfitLossDto> GetProfitLossAsync(Guid chamberId, DateTime from, DateTime to);
    Task<MonthlyReportDto> GetMonthlyReportAsync(Guid chamberId, int year);
    Task<BalanceSheetDto> GetBalanceSheetAsync(Guid chamberId, DateTime asOfDate);
}
