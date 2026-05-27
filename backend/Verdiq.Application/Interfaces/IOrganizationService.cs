using Verdiq.Application.DTOs.Organization;

namespace Verdiq.Application.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationDetailDto> CreateOrganizationAsync(CreateOrganizationDto dto, Guid ownerUserId);
    Task<OrganizationDetailDto> GetOrganizationAsync(Guid organizationId, Guid userId);
    Task<List<OrganizationDto>> GetUserOrganizationsAsync(Guid userId);
    Task<OrganizationDetailDto> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationDto dto, Guid userId);
    Task DeleteOrganizationAsync(Guid organizationId, Guid userId);
    Task<List<OrganizationMemberDto>> GetMembersAsync(Guid organizationId);
    Task<OrganizationMemberDto> InviteMemberAsync(Guid organizationId, InviteMemberDto dto, Guid invitedByUserId);
    Task<OrganizationMemberDto> UpdateMemberRoleAsync(Guid organizationId, Guid memberId, UpdateMemberRoleDto dto, Guid userId);
    Task RemoveMemberAsync(Guid organizationId, Guid memberId, Guid userId);
    Task<WorkspaceDto> CreateWorkspaceAsync(Guid organizationId, CreateWorkspaceDto dto, Guid userId);
    Task<List<WorkspaceDto>> GetWorkspacesAsync(Guid organizationId);
    Task<WorkspaceDto> UpdateWorkspaceAsync(Guid workspaceId, CreateWorkspaceDto dto, Guid userId);
    Task DeleteWorkspaceAsync(Guid workspaceId, Guid userId);
}
