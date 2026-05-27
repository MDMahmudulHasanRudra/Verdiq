using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Organization;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;

    public OrganizationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationDetailDto> CreateOrganizationAsync(CreateOrganizationDto dto, Guid ownerUserId)
    {
        var slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-").Replace(".", "-");

        var org = new Organization
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            Website = dto.Website,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = true,
            OwnerId = ownerUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(org);

        var membership = new OrganizationMember
        {
            OrganizationId = org.Id,
            UserId = ownerUserId,
            Role = OrganizationRole.Owner,
            AcceptedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(membership);
        await _context.SaveChangesAsync();

        return await GetOrganizationAsync(org.Id, ownerUserId);
    }

    public async Task<OrganizationDetailDto> GetOrganizationAsync(Guid organizationId, Guid userId)
    {
        var org = await _context.Organizations
            .Include(o => o.Members).ThenInclude(m => m.User)
            .Include(o => o.Workspaces)
            .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);

        if (org == null)
            throw new KeyNotFoundException("Organization not found");

        var membership = org.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null)
            throw new UnauthorizedAccessException("You are not a member of this organization");

        return MapToDetailDto(org);
    }

    public async Task<List<OrganizationDto>> GetUserOrganizationsAsync(Guid userId)
    {
        var memberships = await _context.OrganizationMembers
            .Include(m => m.Organization)
            .Where(m => m.UserId == userId && !m.Organization.IsDeleted)
            .ToListAsync();

        return memberships.Select(m => new OrganizationDto
        {
            Id = m.Organization.Id,
            Name = m.Organization.Name,
            Slug = m.Organization.Slug,
            Description = m.Organization.Description,
            LogoUrl = m.Organization.LogoUrl,
            IsActive = m.Organization.IsActive,
            CreatedAt = m.Organization.CreatedAt,
            MemberCount = _context.OrganizationMembers.Count(om => om.OrganizationId == m.OrganizationId),
            WorkspaceCount = _context.Workspaces.Count(w => w.OrganizationId == m.OrganizationId)
        }).ToList();
    }

    public async Task<OrganizationDetailDto> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationDto dto, Guid userId)
    {
        var org = await _context.Organizations
            .Include(o => o.Members).ThenInclude(m => m.User)
            .Include(o => o.Workspaces)
            .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);

        if (org == null)
            throw new KeyNotFoundException("Organization not found");

        var membership = org.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || (membership.Role != OrganizationRole.Owner && membership.Role != OrganizationRole.Admin))
            throw new UnauthorizedAccessException("Only owners and admins can update the organization");

        if (dto.Name != null) org.Name = dto.Name;
        if (dto.Slug != null) org.Slug = dto.Slug;
        if (dto.Description != null) org.Description = dto.Description;
        if (dto.Website != null) org.Website = dto.Website;
        if (dto.Address != null) org.Address = dto.Address;
        if (dto.Phone != null) org.Phone = dto.Phone;
        if (dto.Email != null) org.Email = dto.Email;
        org.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDetailDto(org);
    }

    public async Task DeleteOrganizationAsync(Guid organizationId, Guid userId)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);

        if (org == null)
            throw new KeyNotFoundException("Organization not found");

        if (org.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the owner can delete the organization");

        org.IsDeleted = true;
        org.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrganizationMemberDto>> GetMembersAsync(Guid organizationId)
    {
        var members = await _context.OrganizationMembers
            .Include(m => m.User)
            .Where(m => m.OrganizationId == organizationId && !m.IsDeleted)
            .OrderByDescending(m => m.Role)
            .ToListAsync();

        return members.Select(m => new OrganizationMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User.FullName,
            Email = m.User.Email,
            Role = m.Role.ToString(),
            InvitedEmail = m.InvitedEmail,
            InvitedAt = m.InvitedAt,
            AcceptedAt = m.AcceptedAt,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<OrganizationMemberDto> InviteMemberAsync(Guid organizationId, InviteMemberDto dto, Guid invitedByUserId)
    {
        var org = await _context.Organizations.FindAsync(organizationId);
        if (org == null)
            throw new KeyNotFoundException("Organization not found");

        var invitedBy = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == invitedByUserId);
        if (invitedBy == null || invitedBy.Role == OrganizationRole.Viewer)
            throw new UnauthorizedAccessException("You don't have permission to invite members");

        if (!Enum.TryParse<OrganizationRole>(dto.Role, true, out var role))
            role = OrganizationRole.Member;

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        var member = new OrganizationMember
        {
            OrganizationId = organizationId,
            UserId = existingUser?.Id ?? Guid.Empty,
            Role = role,
            InvitedEmail = existingUser == null ? dto.Email : null,
            InvitedAt = DateTime.UtcNow,
            AcceptedAt = existingUser != null ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(member);
        await _context.SaveChangesAsync();

        return new OrganizationMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserName = existingUser?.FullName ?? dto.Email,
            Email = existingUser?.Email ?? dto.Email,
            Role = member.Role.ToString(),
            InvitedEmail = member.InvitedEmail,
            InvitedAt = member.InvitedAt,
            AcceptedAt = member.AcceptedAt,
            CreatedAt = member.CreatedAt
        };
    }

    public async Task<OrganizationMemberDto> UpdateMemberRoleAsync(Guid organizationId, Guid memberId, UpdateMemberRoleDto dto, Guid userId)
    {
        var actor = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);
        if (actor == null || (actor.Role != OrganizationRole.Owner && actor.Role != OrganizationRole.Admin))
            throw new UnauthorizedAccessException("Only owners and admins can change roles");

        var member = await _context.OrganizationMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == organizationId);
        if (member == null)
            throw new KeyNotFoundException("Member not found");

        if (member.Role == OrganizationRole.Owner)
            throw new UnauthorizedAccessException("Cannot change the owner's role");

        if (!Enum.TryParse<OrganizationRole>(dto.Role, true, out var role))
            throw new ArgumentException("Invalid role");

        member.Role = role;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OrganizationMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserName = member.User.FullName,
            Email = member.User.Email,
            Role = member.Role.ToString(),
            CreatedAt = member.CreatedAt
        };
    }

    public async Task RemoveMemberAsync(Guid organizationId, Guid memberId, Guid userId)
    {
        var actor = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);
        if (actor == null || (actor.Role != OrganizationRole.Owner && actor.Role != OrganizationRole.Admin))
            throw new UnauthorizedAccessException("Only owners and admins can remove members");

        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == organizationId);
        if (member == null)
            throw new KeyNotFoundException("Member not found");

        if (member.Role == OrganizationRole.Owner)
            throw new UnauthorizedAccessException("Cannot remove the owner");

        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<WorkspaceDto> CreateWorkspaceAsync(Guid organizationId, CreateWorkspaceDto dto, Guid userId)
    {
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);
        if (membership == null)
            throw new UnauthorizedAccessException("You are not a member of this organization");

        var workspace = new Workspace
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();

        return MapWorkspaceDto(workspace);
    }

    public async Task<List<WorkspaceDto>> GetWorkspacesAsync(Guid organizationId)
    {
        var workspaces = await _context.Workspaces
            .Where(w => w.OrganizationId == organizationId && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync();

        return workspaces.Select(MapWorkspaceDto).ToList();
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(Guid workspaceId, CreateWorkspaceDto dto, Guid userId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspaceId && !w.IsDeleted);
        if (workspace == null)
            throw new KeyNotFoundException("Workspace not found");

        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspace.OrganizationId && m.UserId == userId);
        if (membership == null)
            throw new UnauthorizedAccessException("You are not a member of this organization");

        workspace.Name = dto.Name;
        workspace.Description = dto.Description;
        workspace.Color = dto.Color;
        workspace.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapWorkspaceDto(workspace);
    }

    public async Task DeleteWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspaceId && !w.IsDeleted);
        if (workspace == null)
            throw new KeyNotFoundException("Workspace not found");

        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspace.OrganizationId && m.UserId == userId);
        if (membership == null || (membership.Role != OrganizationRole.Owner && membership.Role != OrganizationRole.Admin))
            throw new UnauthorizedAccessException("Only owners and admins can delete workspaces");

        workspace.IsDeleted = true;
        workspace.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static OrganizationDetailDto MapToDetailDto(Organization org)
    {
        return new OrganizationDetailDto
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            Description = org.Description,
            LogoUrl = org.LogoUrl,
            Website = org.Website,
            Address = org.Address,
            Phone = org.Phone,
            Email = org.Email,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt,
            MemberCount = org.Members?.Count ?? 0,
            WorkspaceCount = org.Workspaces?.Count ?? 0,
            Members = org.Members?
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Role)
                .Select(m => new OrganizationMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = m.User?.FullName ?? m.InvitedEmail ?? "Unknown",
                    Email = m.User?.Email ?? m.InvitedEmail ?? "",
                    Role = m.Role.ToString(),
                    InvitedEmail = m.InvitedEmail,
                    InvitedAt = m.InvitedAt,
                    AcceptedAt = m.AcceptedAt,
                    CreatedAt = m.CreatedAt
                }).ToList() ?? new(),
            Workspaces = org.Workspaces?
                .Where(w => !w.IsDeleted)
                .Select(MapWorkspaceDto).ToList() ?? new()
        };
    }

    private static WorkspaceDto MapWorkspaceDto(Workspace w)
    {
        return new WorkspaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            Color = w.Color,
            OrganizationId = w.OrganizationId,
            CreatedAt = w.CreatedAt
        };
    }
}
