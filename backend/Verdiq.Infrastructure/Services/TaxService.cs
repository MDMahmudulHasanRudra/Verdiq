using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Tax;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class TaxService : ITaxService
{
    private readonly AppDbContext _context;
    public TaxService(AppDbContext context) => _context = context;

    public async Task<TaxSettingResponseDto> CreateTaxSettingAsync(CreateTaxSettingDto dto, Guid chamberId)
    {
        var setting = new TaxSetting
        {
            TaxType = Enum.Parse<TaxType>(dto.TaxType),
            Name = dto.Name, Rate = dto.Rate, Threshold = dto.Threshold,
            Description = dto.Description, ChamberId = chamberId
        };
        _context.Set<TaxSetting>().Add(setting);
        await _context.SaveChangesAsync();
        return MapSetting(setting);
    }

    public async Task<TaxSettingResponseDto> UpdateTaxSettingAsync(Guid id, CreateTaxSettingDto dto)
    {
        var s = await _context.Set<TaxSetting>().FindAsync(id)
            ?? throw new KeyNotFoundException("Tax setting not found");
        s.TaxType = Enum.Parse<TaxType>(dto.TaxType); s.Name = dto.Name;
        s.Rate = dto.Rate; s.Threshold = dto.Threshold;
        s.Description = dto.Description; s.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapSetting(s);
    }

    public async Task DeleteTaxSettingAsync(Guid id)
    {
        var s = await _context.Set<TaxSetting>().FindAsync(id)
            ?? throw new KeyNotFoundException("Tax setting not found");
        s.IsDeleted = true; s.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaxSettingResponseDto>> GetTaxSettingsAsync(Guid chamberId)
    {
        return await _context.Set<TaxSetting>()
            .Where(s => s.ChamberId == chamberId && !s.IsDeleted)
            .Select(s => MapSetting(s)).ToListAsync();
    }

    public async Task<TaxTransactionResponseDto> CreateTaxTransactionAsync(CreateTaxTransactionDto dto, Guid chamberId)
    {
        var setting = await _context.Set<TaxSetting>().FindAsync(dto.TaxSettingId)
            ?? throw new KeyNotFoundException("Tax setting not found");

        var count = await _context.Set<TaxTransaction>().CountAsync(t => t.ChamberId == chamberId);
        var txn = new TaxTransaction
        {
            ReferenceNumber = $"TAX-{DateTime.UtcNow:yyyyMM}-{count + 1:D4}",
            TaxSettingId = dto.TaxSettingId,
            TaxableAmount = dto.TaxableAmount, TaxAmount = dto.TaxAmount,
            TransactionDate = dto.TransactionDate, Month = dto.Month,
            Year = dto.Year, ChallanNo = dto.ChallanNo, Remarks = dto.Remarks,
            ChamberId = chamberId
        };
        _context.Set<TaxTransaction>().Add(txn);
        await _context.SaveChangesAsync();
        return new TaxTransactionResponseDto
        {
            Id = txn.Id, ReferenceNumber = txn.ReferenceNumber,
            TaxSettingId = txn.TaxSettingId,
            TaxTypeName = setting.Name,
            TaxableAmount = txn.TaxableAmount, TaxAmount = txn.TaxAmount,
            TransactionDate = txn.TransactionDate, Month = txn.Month,
            Year = txn.Year, ChallanNo = txn.ChallanNo,
            PaidAt = txn.PaidAt, Remarks = txn.Remarks, CreatedAt = txn.CreatedAt
        };
    }

    public async Task<List<TaxTransactionResponseDto>> GetTaxTransactionsAsync(Guid chamberId, int? year)
    {
        var q = _context.Set<TaxTransaction>().Include(t => t.TaxSetting)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted);
        if (year.HasValue) q = q.Where(t => t.Year == year.Value);
        return await q.OrderByDescending(t => t.TransactionDate)
            .Select(t => new TaxTransactionResponseDto
            {
                Id = t.Id, ReferenceNumber = t.ReferenceNumber,
                TaxSettingId = t.TaxSettingId,
                TaxTypeName = t.TaxSetting.Name,
                TaxableAmount = t.TaxableAmount, TaxAmount = t.TaxAmount,
                TransactionDate = t.TransactionDate, Month = t.Month,
                Year = t.Year, ChallanNo = t.ChallanNo,
                PaidAt = t.PaidAt, Remarks = t.Remarks, CreatedAt = t.CreatedAt
            }).ToListAsync();
    }

    public async Task<decimal> GetTotalTaxLiabilityAsync(Guid chamberId, int year)
    {
        return await _context.Set<TaxTransaction>()
            .Where(t => t.ChamberId == chamberId && t.Year == year && !t.IsDeleted)
            .SumAsync(t => (decimal?)t.TaxAmount) ?? 0;
    }

    private static TaxSettingResponseDto MapSetting(TaxSetting s) => new()
    {
        Id = s.Id, TaxType = s.TaxType.ToString(), Name = s.Name,
        Rate = s.Rate, Threshold = s.Threshold, Description = s.Description,
        IsActive = s.IsActive, CreatedAt = s.CreatedAt
    };
}
