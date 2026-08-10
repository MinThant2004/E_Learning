using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace ELearningManagementSystem.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<PermissionService> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheVersionKey = "permissions-cache-version";

    public PermissionService(IAppDbContext context, ILogger<PermissionService> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    private int GetCacheVersion()
    {
        return _cache.GetOrCreate(CacheVersionKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            return 1;
        });
    }

    public void InvalidatePermissionCache()
    {
        var currentVersion = GetCacheVersion();
        _cache.Set(CacheVersionKey, currentVersion + 1, TimeSpan.FromHours(24));
        _logger.LogInformation("Invalidated user permissions cache. New version is {Version}", currentVersion + 1);
    }

    public async Task<List<string>> GetPermissionsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var version = GetCacheVersion();
        var cacheKey = $"user-permissions-{userId}-v{version}";

        if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
        {
            return cachedPermissions;
        }

        // 1. Verify user exists, is active (Status == true), and not deleted (DeleteFlag == false)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.DeleteFlag && u.Status, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Permissions lookup failed: user not found, inactive, or soft-deleted. UserId={UserId}", userId);
            var empty = new List<string>();
            _cache.Set(cacheKey, empty, TimeSpan.FromMinutes(2));
            return empty;
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

        _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
        return permissions;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var permissions = await GetPermissionsForUserAsync(userId, cancellationToken);
        
        if (permissions.Contains(permissionCode))
            return true;

        if (permissionCode == "Permission.Read" && permissions.Contains("Permission.Assign"))
            return true;

        return false;
    }
}
