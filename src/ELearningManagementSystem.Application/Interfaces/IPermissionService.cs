using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ELearningManagementSystem.Application.Interfaces;

public interface IPermissionService
{
    Task<List<string>> GetPermissionsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);
    void InvalidatePermissionCache();
}
