using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Lead;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly AppDbContext _context;

    public LeadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeadResponseDto>> GetAllAsync(Guid chamberId)
    {
        var leads = await _context.Set<Lead>()
            .Include(l => l.AssignedLawyer)
            .Where(l => l.ChamberId == chamberId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return leads.Select(MapToDto);
    }

    public async Task<LeadResponseDto?> GetByIdAsync(Guid id, Guid chamberId)
    {
        var lead = await _context.Set<Lead>()
            .Include(l => l.AssignedLawyer)
            .FirstOrDefaultAsync(l => l.Id == id && l.ChamberId == chamberId && !l.IsDeleted);

        return lead == null ? null : MapToDto(lead);
    }

    public async Task<LeadResponseDto> CreateAsync(CreateLeadDto dto, Guid chamberId, Guid userId)
    {
        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            ChamberId = chamberId,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            CompanyName = dto.CompanyName,
            CaseType = dto.CaseType,
            EstimatedValue = dto.EstimatedValue,
            LeadSource = dto.LeadSource,
            Stage = "NewLead",
            AssignedLawyerId = dto.AssignedLawyerId,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate,
            LastContactedAt = now,
            CreatedAt = now
        };

        _context.Set<Lead>().Add(lead);
        await _context.SaveChangesAsync();

        return MapToDto(lead);
    }

    public async Task<LeadResponseDto?> UpdateAsync(Guid id, UpdateLeadDto dto, Guid chamberId)
    {
        var lead = await _context.Set<Lead>()
            .Include(l => l.AssignedLawyer)
            .FirstOrDefaultAsync(l => l.Id == id && l.ChamberId == chamberId && !l.IsDeleted);

        if (lead == null) return null;

        if (dto.Name != null) lead.Name = dto.Name;
        if (dto.Phone != null) lead.Phone = dto.Phone;
        if (dto.Email != null) lead.Email = dto.Email;
        if (dto.CompanyName != null) lead.CompanyName = dto.CompanyName;
        if (dto.CaseType != null) lead.CaseType = dto.CaseType;
        if (dto.EstimatedValue.HasValue) lead.EstimatedValue = dto.EstimatedValue.Value;
        if (dto.LeadSource != null) lead.LeadSource = dto.LeadSource;
        if (dto.Notes != null) lead.Notes = dto.Notes;
        if (dto.FollowUpDate.HasValue) lead.FollowUpDate = dto.FollowUpDate;
        lead.AssignedLawyerId = dto.AssignedLawyerId ?? lead.AssignedLawyerId;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(lead);
    }

    public async Task<LeadResponseDto?> UpdateStageAsync(Guid id, UpdateLeadStageDto dto, Guid chamberId)
    {
        var lead = await _context.Set<Lead>()
            .Include(l => l.AssignedLawyer)
            .FirstOrDefaultAsync(l => l.Id == id && l.ChamberId == chamberId && !l.IsDeleted);

        if (lead == null) return null;

        lead.Stage = dto.Stage;
        lead.UpdatedAt = DateTime.UtcNow;

        if (dto.Stage == "ConvertedToClient")
        {
            lead.ConvertedAt = DateTime.UtcNow;
        }

        if (dto.Stage == "LostLead" && !string.IsNullOrEmpty(dto.LostReason))
        {
            lead.LostReason = dto.LostReason;
        }

        await _context.SaveChangesAsync();
        return MapToDto(lead);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid chamberId)
    {
        var lead = await _context.Set<Lead>()
            .FirstOrDefaultAsync(l => l.Id == id && l.ChamberId == chamberId && !l.IsDeleted);

        if (lead == null) return false;

        lead.IsDeleted = true;
        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LeadAnalyticsDto> GetAnalyticsAsync(Guid chamberId)
    {
        var leads = await _context.Set<Lead>()
            .Where(l => l.ChamberId == chamberId && !l.IsDeleted)
            .ToListAsync();

        var analytics = new LeadAnalyticsDto
        {
            TotalLeads = leads.Count,
            NewLeads = leads.Count(l => l.Stage == "NewLead"),
            ConsultationScheduled = leads.Count(l => l.Stage == "ConsultationScheduled"),
            FollowUpPending = leads.Count(l => l.Stage == "FollowUpPending"),
            ProposalSent = leads.Count(l => l.Stage == "ProposalSent"),
            Converted = leads.Count(l => l.Stage == "ConvertedToClient"),
            Lost = leads.Count(l => l.Stage == "LostLead"),
            TotalEstimatedValue = leads.Sum(l => l.EstimatedValue),
            ConvertedValue = leads.Where(l => l.Stage == "ConvertedToClient").Sum(l => l.EstimatedValue),
            ConversionRate = leads.Count > 0
                ? Math.Round((double)leads.Count(l => l.Stage == "ConvertedToClient") / leads.Count * 100, 1)
                : 0,
            AverageConversionDays = leads.Where(l => l.ConvertedAt != default).Any()
                ? leads.Where(l => l.ConvertedAt != default)
                    .Average(l => (l.ConvertedAt - l.CreatedAt).TotalDays)
                : 0,
            BySource = leads.GroupBy(l => l.LeadSource)
                .Select(g => new SourceBreakdown
                {
                    Source = g.Key,
                    Count = g.Count(),
                    Value = g.Sum(l => l.EstimatedValue)
                }).ToList()
        };

        return analytics;
    }

    public async Task<IEnumerable<LeadResponseDto>> GetByStageAsync(string stage, Guid chamberId)
    {
        var leads = await _context.Set<Lead>()
            .Include(l => l.AssignedLawyer)
            .Where(l => l.ChamberId == chamberId && l.Stage == stage && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return leads.Select(MapToDto);
    }

    private static LeadResponseDto MapToDto(Lead l)
    {
        var daysSinceContact = (DateTime.UtcNow - (l.LastContactedAt ?? l.CreatedAt)).TotalDays;

        return new LeadResponseDto
        {
            Id = l.Id,
            Name = l.Name,
            Phone = l.Phone,
            Email = l.Email,
            CompanyName = l.CompanyName,
            CaseType = l.CaseType,
            EstimatedValue = l.EstimatedValue,
            LeadSource = l.LeadSource,
            Stage = l.Stage,
            AssignedLawyerId = l.AssignedLawyerId,
            AssignedLawyerName = l.AssignedLawyer?.FullName,
            Notes = l.Notes,
            FollowUpDate = l.FollowUpDate,
            LastContactedAt = l.LastContactedAt,
            Score = CalculateScore(l, daysSinceContact),
            IsStale = daysSinceContact > 7,
            CreatedAt = l.CreatedAt,
            ConvertedAt = l.ConvertedAt == default ? null : l.ConvertedAt,
            LostReason = l.LostReason,
        };
    }

    private static int CalculateScore(Lead l, double daysSinceContact)
    {
        var score = 0;
        if (l.EstimatedValue >= 50000) score += 30;
        else if (l.EstimatedValue >= 20000) score += 20;
        else if (l.EstimatedValue >= 5000) score += 10;

        if (daysSinceContact <= 2) score += 25;
        else if (daysSinceContact <= 5) score += 15;
        else if (daysSinceContact <= 7) score += 5;

        if (l.Stage == "ConsultationScheduled") score += 20;
        else if (l.Stage == "ProposalSent") score += 15;
        else if (l.Stage == "FollowUpPending") score += 10;

        if (l.FollowUpDate.HasValue && l.FollowUpDate > DateTime.UtcNow) score += 10;

        return Math.Min(score, 100);
    }
}
