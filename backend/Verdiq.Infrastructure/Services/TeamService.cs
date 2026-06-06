using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Team;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly AppDbContext _context;

    public TeamService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeamResponseDto> CreateAsync(CreateTeamDto dto, Guid userId, Guid chamberId)
    {
        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            ChamberId = chamberId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);

        if (dto.MemberIds?.Count > 0)
        {
            foreach (var memberId in dto.MemberIds.Distinct())
            {
                _context.Set<TeamMember>().Add(new TeamMember
                {
                    TeamId = team.Id,
                    UserId = memberId,
                    Role = memberId == userId ? "Lead" : "Member",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(team.Id))!;
    }

    public async Task<TeamResponseDto> UpdateAsync(Guid id, UpdateTeamDto dto)
    {
        var team = await _context.Teams.FindAsync(id)
            ?? throw new KeyNotFoundException("Team not found");

        if (dto.Name != null) team.Name = dto.Name;
        if (dto.Description != null) team.Description = dto.Description;
        team.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted)
            ?? throw new KeyNotFoundException("Team not found");

        team.IsDeleted = true;
        team.UpdatedAt = DateTime.UtcNow;

        foreach (var member in team.Members)
            member.IsDeleted = true;

        await _context.SaveChangesAsync();
    }

    public async Task<TeamResponseDto?> GetByIdAsync(Guid id)
    {
        var team = await _context.Teams
            .Include(t => t.CreatedBy)
            .Include(t => t.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (team == null) return null;

        return MapToDto(team);
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllAsync(Guid chamberId)
    {
        var teams = await _context.Teams
            .Include(t => t.CreatedBy)
            .Include(t => t.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return teams.Select(MapToDto);
    }

    public async Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddTeamMemberDto dto)
    {
        var team = await _context.Teams.FindAsync(teamId)
            ?? throw new KeyNotFoundException("Team not found");

        var existing = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == dto.UserId && !m.IsDeleted);

        if (existing != null)
            throw new InvalidOperationException("User is already a member of this team");

        var member = new TeamMember
        {
            TeamId = teamId,
            UserId = dto.UserId,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<TeamMember>().Add(member);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(dto.UserId)!;
        return new TeamMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserName = user?.FullName ?? "Unknown",
            UserEmail = user?.Email ?? "",
            UserRole = user?.Role.ToString() ?? "",
            AvatarUrl = user?.AvatarUrl,
            TeamRole = member.Role,
            JoinedAt = member.CreatedAt
        };
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId)
    {
        var member = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Member not found");

        member.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<TeamMemberDto> UpdateMemberRoleAsync(Guid teamId, Guid userId, UpdateTeamMemberRoleDto dto)
    {
        var member = await _context.Set<TeamMember>()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Member not found");

        member.Role = dto.Role;
        await _context.SaveChangesAsync();

        return new TeamMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserName = member.User?.FullName ?? "Unknown",
            UserEmail = member.User?.Email ?? "",
            UserRole = member.User?.Role.ToString() ?? "",
            AvatarUrl = member.User?.AvatarUrl,
            TeamRole = member.Role,
            JoinedAt = member.CreatedAt
        };
    }

    private static TeamResponseDto MapToDto(Team t)
    {
        return new TeamResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            MemberCount = t.Members?.Count(m => !m.IsDeleted) ?? 0,
            CreatedAt = t.CreatedAt,
            CreatedByName = t.CreatedBy?.FullName ?? "Unknown",
            Members = t.Members?
                .Where(m => !m.IsDeleted)
                .Select(m => new TeamMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = m.User?.FullName ?? "Unknown",
                    UserEmail = m.User?.Email ?? "",
                    UserRole = m.User?.Role.ToString() ?? "",
                    AvatarUrl = m.User?.AvatarUrl,
                    TeamRole = m.Role,
                    JoinedAt = m.CreatedAt
                }).ToList() ?? new List<TeamMemberDto>()
        };
    }
}
