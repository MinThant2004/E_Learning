using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IAppDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<string>> GetPermissionsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // 1. Verify user exists, is active (Status == true), and not deleted (DeleteFlag == false)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.DeleteFlag && u.Status, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Permissions lookup failed: user not found, inactive, or soft-deleted. UserId={UserId}", userId);
            return new List<string>();
        }

        // 2. Query distinct active permission codes for this user's roles
        var permissions = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Resolved {Count} permissions for UserId={UserId}", permissions.Count, userId);

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var permissions = await GetPermissionsForUserAsync(userId, cancellationToken);
        return permissions.Contains(permissionCode);
    }
}
