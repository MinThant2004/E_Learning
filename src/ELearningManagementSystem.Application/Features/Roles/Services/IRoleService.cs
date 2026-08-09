using System.Collections.Generic;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Roles.DTOs;

namespace ELearningManagementSystem.Application.Features.Roles.Services;

public interface IRoleService
{
    Task<PagedResult<RoleResponse>> GetRolesAsync(RoleListQuery query);
    Task<Result<RoleDetailResponse>> GetRoleByIdAsync(int roleId);
    Task<Result<int>> CreateRoleAsync(CreateRoleRequest request);
    Task<Result<bool>> UpdateRoleAsync(int roleId, UpdateRoleRequest request);
    Task<Result<bool>> AssignPermissionsAsync(int roleId, AssignPermissionsRequest request);
}
