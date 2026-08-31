using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Users.DTOs;

namespace ELearningManagementSystem.Application.Features.Users.Services;

public interface IUserService
{
    Task<Result<int>> CreateUserAsync(CreateUserRequest request, int currentUserId);
    Task<PagedResult<AdminUserSummaryResponse>> GetPagedUsersAsync(UserListQuery query);
    Task<Result<AdminUserDetailResponse>> GetUserByIdAsync(int userId);
    Task<Result<bool>> UpdateUserAsync(int userId, UpdateUserRequest request, int currentUserId);
    Task<Result<bool>> ArchiveUserAsync(int userId, int currentUserId);
    Task<Result<bool>> RestoreUserAsync(int userId, int currentUserId);
    Task<Result<List<string>>> GetAvailableRolesAsync();
    Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
