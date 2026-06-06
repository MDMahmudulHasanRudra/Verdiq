using Verdiq.Application.DTOs.Team;

namespace Verdiq.Application.Interfaces;

public interface ITeamService
{
    Task<TeamResponseDto> CreateAsync(CreateTeamDto dto, Guid userId, Guid chamberId);
    Task<TeamResponseDto> UpdateAsync(Guid id, UpdateTeamDto dto);
    Task DeleteAsync(Guid id);
    Task<TeamResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TeamResponseDto>> GetAllAsync(Guid chamberId);
    Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddTeamMemberDto dto);
    Task RemoveMemberAsync(Guid teamId, Guid userId);
    Task<TeamMemberDto> UpdateMemberRoleAsync(Guid teamId, Guid userId, UpdateTeamMemberRoleDto dto);
}
