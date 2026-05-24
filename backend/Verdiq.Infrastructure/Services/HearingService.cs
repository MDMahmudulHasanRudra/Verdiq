using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Hearing;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
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

    public async Task<HearingResponseDto> GetHearingByIdAsync(Guid id)
    {
        var hearing = await _context.Hearings
            .Include(h => h.Case)
            .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

        if (hearing == null)
            throw new KeyNotFoundException("Hearing not found");

        return MapToDto(hearing);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetHearingsByCaseIdAsync(Guid caseId)
    {
        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Where(h => h.CaseId == caseId && !h.IsDeleted)
            .OrderByDescending(h => h.HearingDate)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetUpcomingHearingsAsync(Guid lawyerId)
    {
        var now = DateTime.UtcNow;
        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Where(h => h.Case.AssignedLawyerId == lawyerId
                && h.HearingDate >= now
                && h.Status == HearingStatus.Scheduled
                && !h.IsDeleted)
            .OrderBy(h => h.HearingDate)
            .ThenBy(h => h.Time)
            .Take(10)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    public async Task<IEnumerable<HearingResponseDto>> GetHearingsByDateAsync(DateTime date, Guid lawyerId)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var hearings = await _context.Hearings
            .Include(h => h.Case)
            .Where(h => h.Case.AssignedLawyerId == lawyerId
                && h.HearingDate >= dayStart
                && h.HearingDate < dayEnd
                && !h.IsDeleted)
            .OrderBy(h => h.Time)
            .ToListAsync();

        return hearings.Select(MapToDto);
    }

    public async Task<HearingResponseDto> CreateHearingAsync(CreateHearingDto dto)
    {
        var caseEntity = await _context.Cases.FindAsync(dto.CaseId);
        if (caseEntity == null || caseEntity.IsDeleted)
            throw new KeyNotFoundException("Case not found");

        var hearing = new Hearing
        {
            CaseId = dto.CaseId,
            HearingDate = dto.HearingDate,
            Time = dto.Time,
            Court = dto.Court,
            CourtRoom = dto.CourtRoom,
            JudgeName = dto.JudgeName,
            HearingType = dto.HearingType,
            Notes = dto.Notes,
            Status = HearingStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Hearings.AddAsync(hearing);
        await _unitOfWork.CompleteAsync();

        return await GetHearingByIdAsync(hearing.Id);
    }

    public async Task<HearingResponseDto> UpdateHearingAsync(Guid id, UpdateHearingDto dto)
    {
        var hearing = await _context.Hearings.FindAsync(id);
        if (hearing == null || hearing.IsDeleted)
            throw new KeyNotFoundException("Hearing not found");

        if (dto.HearingDate.HasValue) hearing.HearingDate = dto.HearingDate.Value;
        if (dto.Time != null) hearing.Time = dto.Time;
        if (dto.Status != null && Enum.TryParse<HearingStatus>(dto.Status, true, out var status))
            hearing.Status = status;
        if (dto.Notes != null) hearing.Notes = dto.Notes;

        await _unitOfWork.Hearings.UpdateAsync(hearing);
        await _unitOfWork.CompleteAsync();

        return await GetHearingByIdAsync(id);
    }

    public async Task DeleteHearingAsync(Guid id)
    {
        var hearing = await _context.Hearings.FindAsync(id);
        if (hearing == null || hearing.IsDeleted)
            throw new KeyNotFoundException("Hearing not found");

        await _unitOfWork.Hearings.DeleteAsync(hearing);
        await _unitOfWork.CompleteAsync();
    }

    public async Task SendReminderAsync(Guid hearingId)
    {
        var hearing = await _context.Hearings
            .Include(h => h.Case)
            .ThenInclude(c => c.AssignedLawyer)
            .FirstOrDefaultAsync(h => h.Id == hearingId);

        if (hearing == null || hearing.ReminderSent) return;

        var notification = new Notification
        {
            UserId = hearing.Case.AssignedLawyerId,
            Title = "Upcoming Hearing Reminder",
            Message = $"Hearing for {hearing.Case.CaseNumber} - {hearing.Case.Title} is scheduled on {hearing.HearingDate:yyyy-MM-dd} at {hearing.Time} in {hearing.Court}",
            Type = "hearing",
            IsRead = false,
            ReferenceId = hearing.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        hearing.ReminderSent = true;
        await _unitOfWork.Hearings.UpdateAsync(hearing);
        await _unitOfWork.CompleteAsync();
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
            Time = h.Time,
            Court = h.Court,
            CourtRoom = h.CourtRoom,
            JudgeName = h.JudgeName,
            HearingType = h.HearingType,
            Status = h.Status.ToString(),
            Notes = h.Notes,
            CreatedAt = h.CreatedAt
        };
    }
}
