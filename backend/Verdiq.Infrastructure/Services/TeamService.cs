using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Team;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
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
                    CreatedAt = DateTime.UtcNow,
                    AcceptedAt = DateTime.UtcNow
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

        if (dto.UserId.HasValue)
        {
            var existing = await _context.Set<TeamMember>()
                .AnyAsync(m => m.TeamId == teamId && m.UserId == dto.UserId && !m.IsDeleted);

            if (existing)
                throw new InvalidOperationException("User is already a member of this team");

            var member = new TeamMember
            {
                TeamId = teamId,
                UserId = dto.UserId,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                AcceptedAt = DateTime.UtcNow
            };

            _context.Set<TeamMember>().Add(member);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId.Value);
            return new TeamMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                UserName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "",
                UserRole = user?.Role.ToString() ?? "",
                AvatarUrl = user?.AvatarUrl,
                TeamRole = member.Role,
                JoinedAt = member.CreatedAt,
                IsPending = false
            };
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new InvalidOperationException("Either UserId or Email must be provided");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("Password is required when creating a new user");

        if (dto.Password.Length < 6)
            throw new InvalidOperationException("Password must be at least 6 characters");

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists");

        if (!Enum.TryParse<UserRole>(dto.UserRole ?? "JuniorLawyer", true, out var userRole))
            throw new InvalidOperationException("Invalid user role");

        if (string.IsNullOrWhiteSpace(dto.InvitedName))
            throw new InvalidOperationException("Name is required");

        var newUser = new User
        {
            FullName = dto.InvitedName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = "",
            Role = userRole,
            ChamberId = team.ChamberId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);

        var newMember = new TeamMember
        {
            TeamId = teamId,
            UserId = newUser.Id,
            InvitedName = dto.InvitedName,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow
        };

        _context.Set<TeamMember>().Add(newMember);
        await _context.SaveChangesAsync();

        return new TeamMemberDto
        {
            Id = newMember.Id,
            UserId = newMember.UserId,
            UserName = newUser.FullName,
            UserEmail = newUser.Email,
            UserRole = newUser.Role.ToString(),
            AvatarUrl = newUser.AvatarUrl,
            TeamRole = newMember.Role,
            JoinedAt = newMember.CreatedAt,
            IsPending = false
        };
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid memberId)
    {
        var member = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.Id == memberId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Member not found");

        member.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<TeamMemberDto> UpdateMemberRoleAsync(Guid teamId, Guid memberId, UpdateTeamMemberRoleDto dto)
    {
        var member = await _context.Set<TeamMember>()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.Id == memberId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Member not found");

        member.Role = dto.Role;
        await _context.SaveChangesAsync();

        return new TeamMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserName = member.User?.FullName ?? member.InvitedName ?? "Pending",
            UserEmail = member.User?.Email ?? member.Email ?? "",
            UserRole = member.User?.Role.ToString() ?? "",
            AvatarUrl = member.User?.AvatarUrl,
            TeamRole = member.Role,
            JoinedAt = member.CreatedAt,
            IsPending = member.UserId == null,
            InvitedName = member.InvitedName
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
                    UserName = m.User?.FullName ?? m.InvitedName ?? "Pending",
                    UserEmail = m.User?.Email ?? m.Email ?? "",
                    UserRole = m.User?.Role.ToString() ?? "",
                    AvatarUrl = m.User?.AvatarUrl,
                    TeamRole = m.Role,
                    JoinedAt = m.CreatedAt,
                    IsPending = m.UserId == null,
                    InvitedName = m.InvitedName
                }).ToList() ?? new List<TeamMemberDto>()
        };
    }
}
