using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Hearing;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class HearingService : IHearingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public HearingService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<(bool Success, string Message, HearingResponseDto? Data)> CreateAsync(CreateHearingDto dto, Guid chamberId)
    {
        var caseEntity = await _context.Cases.FindAsync(dto.CaseId);
        if (caseEntity == null || caseEntity.IsDeleted || caseEntity.ChamberId != chamberId)
            return (false, "Case not found", null);

        var hearing = new Hearing
        {
            CaseId = dto.CaseId,
            HearingDate = DateTime.SpecifyKind(dto.HearingDate, DateTimeKind.Utc),
            Courtroom = dto.Courtroom,
            JudgeName = dto.JudgeName,
            Notes = dto.Notes,
            Status = Domain.Enums.HearingStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Hearing>().AddAsync(hearing);
        await _unitOfWork.CompleteAsync();

        var result = await GetByIdAsync(hearing.Id);
        return (true, "Hearing created successfully", result);
    }

    public async Task<(bool Success, string Message, HearingResponseDto? Data)> UpdateAsync(Guid id, UpdateHearingDto dto)
    {
        var hearing = await _context.Hearings.FindAsync(id);
        if (hearing == null || hearing.IsDeleted)
            return (false, "Hearing not found", null);

        if (dto.HearingDate.HasValue) hearing.HearingDate = DateTime.SpecifyKind(dto.HearingDate.Value, DateTimeKind.Utc);
        if (dto.Courtroom != null) hearing.Courtroom = dto.Courtroom;
        if (dto.JudgeName != null) hearing.JudgeName = dto.JudgeName;
        if (dto.Result != null) hearing.Result = dto.Result;
        if (dto.NextHearingDate.HasValue) hearing.NextHearingDate = DateTime.SpecifyKind(dto.NextHearingDate.Value, DateTimeKind.Utc);
        if (dto.Notes != null) hearing.Notes = dto.Notes;
        if (dto.Status != null && Enum.TryParse<Domain.Enums.HearingStatus>(dto.Status, true, out var status))
            hearing.Status = status;

        hearing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Hearing>().UpdateAsync(hearing);
        await _unitOfWork.CompleteAsync();

        var result = await GetByIdAsync(id);
        return (true, "Hearing updated successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var hearing = await _context.Hearings.FindAsync(id);
        if (hearing == null || hearing.IsDeleted)
            return (false, "Hearing not found");

        await _unitOfWork.Repository<Hearing>().DeleteAsync(hearing);
        await _unitOfWork.CompleteAsync();

        return (true, "Hearing deleted successfully");
    }

    public async Task<HearingResponseDto?> GetByIdAsync(Guid id)
    {
        var hearing = await _context.Hearings
            .Include(h => h.Case)
            .Include(h => h.Tasks)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

        return hearing == null ? null : MapToDto(hearing);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetByCaseIdAsync(Guid caseId)
    {
        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Include(h => h.Tasks)
            .Where(h => h.CaseId == caseId && !h.IsDeleted)
            .OrderByDescending(h => h.HearingDate)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetUpcomingAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Include(h => h.Tasks)
            .Where(h => h.Case.ChamberId == chamberId
                && h.HearingDate >= now
                && h.Status == Domain.Enums.HearingStatus.Scheduled
                && !h.IsDeleted)
            .OrderBy(h => h.HearingDate)
            .Take(10)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetByDateAsync(DateTime date, Guid chamberId)
    {
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Include(h => h.Tasks)
            .Where(h => h.Case.ChamberId == chamberId
                && h.HearingDate >= dayStart
                && h.HearingDate < dayEnd
                && !h.IsDeleted)
            .OrderBy(h => h.HearingDate)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    private static HearingResponseDto MapToDto(Hearing h)
    {
        return new HearingResponseDto
        {
            Id = h.Id,
            CaseId = h.CaseId,
            CaseNumber = h.Case.CaseNumber,
            CaseTitle = h.Case.Title,
            HearingDate = h.HearingDate,
            Courtroom = h.Courtroom,
            JudgeName = h.JudgeName,
            Result = h.Result,
            NextHearingDate = h.NextHearingDate,
            Status = h.Status.ToString(),
            Notes = h.Notes,
            CreatedAt = h.CreatedAt,
            HasIncompletePreHearingTasks = h.Tasks?.Any(t =>
                !t.IsDeleted && t.IsPreHearing &&
                t.Status != Domain.Enums.TaskStatus.Completed &&
                t.Status != Domain.Enums.TaskStatus.Cancelled) ?? false,
            HasPreHearingTasks = h.Tasks?.Any(t =>
                !t.IsDeleted && t.IsPreHearing) ?? false
        };
    }
}
