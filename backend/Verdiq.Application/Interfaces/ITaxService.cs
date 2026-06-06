using Verdiq.Application.DTOs.Tax;

namespace Verdiq.Application.Interfaces;

public interface ITaxService
{
    Task<TaxSettingResponseDto> CreateTaxSettingAsync(CreateTaxSettingDto dto, Guid chamberId);
    Task<TaxSettingResponseDto> UpdateTaxSettingAsync(Guid id, CreateTaxSettingDto dto);
    Task DeleteTaxSettingAsync(Guid id);
    Task<List<TaxSettingResponseDto>> GetTaxSettingsAsync(Guid chamberId);
    Task<TaxTransactionResponseDto> CreateTaxTransactionAsync(CreateTaxTransactionDto dto, Guid chamberId);
    Task<List<TaxTransactionResponseDto>> GetTaxTransactionsAsync(Guid chamberId, int? year);
    Task<decimal> GetTotalTaxLiabilityAsync(Guid chamberId, int year);
}
