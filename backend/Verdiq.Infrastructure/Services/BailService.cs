using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Bail;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class BailService : IBailService
{
    private readonly AppDbContext _context;

    public BailService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string Message, BailResponseDto? Data)> CreateAsync(CreateBailDto dto, Guid chamberId)
    {
        var caseEntity = await _context.Cases.FindAsync(dto.CaseId);
        if (caseEntity == null || caseEntity.IsDeleted || caseEntity.ChamberId != chamberId)
            return (false, "Case not found", null);

        var existing = await _context.Bails.AnyAsync(b => b.CaseId == dto.CaseId && !b.IsDeleted);
        if (existing)
            return (false, "A bail record already exists for this case", null);

        var bail = new Bail
        {
            CaseId = dto.CaseId,
            Status = BailStatus.Pending,
            BailAmount = dto.BailAmount,
            BailConditions = dto.BailConditions,
            BailHearingDate = dto.BailHearingDate.HasValue ? DateTime.SpecifyKind(dto.BailHearingDate.Value, DateTimeKind.Utc) : null,
            BondNumber = dto.BondNumber,
            SuretyName = dto.SuretyName,
            SuretyAddress = dto.SuretyAddress,
            SuretyContact = dto.SuretyContact,
            GrantedBy = dto.GrantedBy,
            Notes = dto.Notes,
        };

        _context.Bails.Add(bail);
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(bail.Id);
        return (true, "Bail record created", result);
    }

    public async Task<(bool Success, string Message, BailResponseDto? Data)> UpdateAsync(Guid id, UpdateBailDto dto)
    {
        var bail = await _context.Bails.FindAsync(id);
        if (bail == null || bail.IsDeleted)
            return (false, "Bail record not found", null);

        if (dto.BailAmount.HasValue) bail.BailAmount = dto.BailAmount;
        if (dto.BailConditions != null) bail.BailConditions = dto.BailConditions;
        if (dto.BailHearingDate.HasValue) bail.BailHearingDate = DateTime.SpecifyKind(dto.BailHearingDate.Value, DateTimeKind.Utc);
        if (dto.BondNumber != null) bail.BondNumber = dto.BondNumber;
        if (dto.SuretyName != null) bail.SuretyName = dto.SuretyName;
        if (dto.SuretyAddress != null) bail.SuretyAddress = dto.SuretyAddress;
        if (dto.SuretyContact != null) bail.SuretyContact = dto.SuretyContact;
        if (dto.GrantedBy != null) bail.GrantedBy = dto.GrantedBy;
        if (dto.Notes != null) bail.Notes = dto.Notes;

        bail.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, "Bail record updated", result);
    }

    public async Task<(bool Success, string Message, BailResponseDto? Data)> UpdateStatusAsync(Guid id, UpdateBailStatusDto dto)
    {
        var bail = await _context.Bails.Include(b => b.Case).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        if (bail == null)
            return (false, "Bail record not found", null);

        if (!Enum.TryParse<BailStatus>(dto.Status, true, out var status))
            return (false, "Invalid bail status", null);

        bail.Status = status;
        bail.UpdatedAt = DateTime.UtcNow;

        if (status == BailStatus.Granted)
            bail.BailGrantedAt = DateTime.UtcNow;
        else if (status == BailStatus.Revoked)
        {
            bail.RevokedAt = DateTime.UtcNow;
            bail.RevokedReason = dto.RevokedReason;
        }

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = bail.CaseId,
            ActivityType = ActivityType.StatusChange,
            Description = $"Bail status changed to {status}",
        });

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return (true, $"Bail status changed to {status}", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var bail = await _context.Bails.FindAsync(id);
        if (bail == null || bail.IsDeleted)
            return (false, "Bail record not found");

        bail.IsDeleted = true;
        bail.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Bail record deleted");
    }

    public async Task<BailResponseDto?> GetByIdAsync(Guid id)
    {
        var bail = await _context.Bails
            .Include(b => b.Case)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        return bail == null ? null : MapToDto(bail);
    }

    public async Task<BailResponseDto?> GetByCaseIdAsync(Guid caseId)
    {
        var bail = await _context.Bails
            .Include(b => b.Case)
            .FirstOrDefaultAsync(b => b.CaseId == caseId && !b.IsDeleted);

        return bail == null ? null : MapToDto(bail);
    }

    public async Task<IEnumerable<BailResponseDto>> GetAllAsync(Guid chamberId, string? status = null)
    {
        var query = _context.Bails
            .Include(b => b.Case)
            .Where(b => b.Case.ChamberId == chamberId && !b.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BailStatus>(status, true, out var bailStatus))
            query = query.Where(b => b.Status == bailStatus);

        return await query.OrderByDescending(b => b.CreatedAt).Select(b => MapToDto(b)).ToListAsync();
    }

    private static BailResponseDto MapToDto(Bail b) => new()
    {
        Id = b.Id,
        CaseId = b.CaseId,
        CaseNumber = b.Case.CaseNumber,
        CaseTitle = b.Case.Title,
        Status = b.Status.ToString(),
        BailAmount = b.BailAmount,
        BailConditions = b.BailConditions,
        BailGrantedAt = b.BailGrantedAt,
        BailHearingDate = b.BailHearingDate,
        BondNumber = b.BondNumber,
        SuretyName = b.SuretyName,
        SuretyAddress = b.SuretyAddress,
        SuretyContact = b.SuretyContact,
        RevokedAt = b.RevokedAt,
        RevokedReason = b.RevokedReason,
        GrantedBy = b.GrantedBy,
        Notes = b.Notes,
        CreatedAt = b.CreatedAt,
    };
}
