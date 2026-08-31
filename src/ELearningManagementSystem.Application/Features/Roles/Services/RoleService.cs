using ELearningManagementSystem.Shared.Constants;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Application.Features.Roles.DTOs;
using ELearningManagementSystem.Domain.Entities;

namespace ELearningManagementSystem.Application.Features.Roles.Services;

public class RoleService : IRoleService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RoleService> _logger;
    private readonly IPermissionService _permissionService;

    public RoleService(IAppDbContext context, ICurrentUserService currentUserService, ILogger<RoleService> logger, IPermissionService permissionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _permissionService = permissionService;
    }

    public async Task<PagedResult<RoleResponse>> GetRolesAsync(RoleListQuery query)
    {
        var dbQuery = _context.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            dbQuery = dbQuery.Where(r => r.RoleName.Contains(query.SearchTerm) || 
                                         (r.Description != null && r.Description.Contains(query.SearchTerm)));
        }

        var totalCount = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderBy(r => r.RoleName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<RoleResponse>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<Result<RoleDetailResponse>> GetRoleByIdAsync(int roleId)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);

        if (role == null) return Result.Failure<RoleDetailResponse>("RoleNotFound");

        var response = new RoleDetailResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
        };

        return Result.Success<RoleDetailResponse>(response);
    }

    public async Task<Result<int>> CreateRoleAsync(CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result.Failure<int>("Role name is required");

        bool exists = await _context.Roles.AnyAsync(r => r.RoleName == request.RoleName);
        if (exists) return Result.Failure<int>("RoleAlreadyExists");

        if (request.RoleName.Equals(AppConstants.SystemAdministratorRole, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<int>("CannotCreateSuperAdminRole: The 'System Administrator' role name is reserved.");
        }

        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Result.Failure<int>("Unauthorized");

        bool isCallerSuperAdmin = await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == currentUserId.Value && ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        if (!isCallerSuperAdmin && request.PermissionIds != null && request.PermissionIds.Any())
        {
            var callerPermissionIds = await _context.UserRoles
                .Where(ur => ur.UserId == currentUserId.Value)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToListAsync();

            var unauthorizedPermissionIds = request.PermissionIds.Except(callerPermissionIds).ToList();
            if (unauthorizedPermissionIds.Any())
            {
                return Result.Failure<int>("PrivilegeEscalation: You cannot assign permissions that you do not possess.");
            }
        }

        var role = new Role
        {
            RoleName = request.RoleName,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            foreach (var permId in request.PermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.RoleId,
                    PermissionId = permId
                });
            }
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("User {UserId} created Role {RoleName} (RoleId={RoleId}) with {PermissionCount} permissions", currentUserId.Value, role.RoleName, role.RoleId, request.PermissionIds?.Count ?? 0);

        return Result.Success<int>(role.RoleId);
    }

    public async Task<Result<bool>> UpdateRoleAsync(int roleId, UpdateRoleRequest request)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null) return Result.Failure<bool>("RoleNotFound");

        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Result.Failure<bool>("Unauthorized");

        bool isCallerSuperAdmin = await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == currentUserId.Value && ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        if ((role.RoleName == AppConstants.SystemAdministratorRole || role.RoleName == AppConstants.AdministratorRole) && !isCallerSuperAdmin)
        {
            return Result.Failure<bool>("ProtectedRole: Only a System Administrator can modify built-in administrator roles.");
        }
        
        // Prevent editing the name of built-in roles like "Student", "Admin", and AppConstants.SystemAdministratorRole
        if (role.RoleName == "Student" || role.RoleName == AppConstants.SystemAdministratorRole || role.RoleName == AppConstants.AdministratorRole)
        {
            if (!string.Equals(role.RoleName, request.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<bool>($"Cannot change the name of the built-in role '{role.RoleName}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result.Failure<bool>("Role name is required");

        if (role.RoleName != request.RoleName)
        {
            bool exists = await _context.Roles.AnyAsync(r => r.RoleName == request.RoleName && r.RoleId != roleId);
            if (exists) return Result.Failure<bool>("RoleAlreadyExists");
        }

        role.RoleName = request.RoleName;
        role.Description = request.Description;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated Role {RoleId} to Name: {RoleName}", currentUserId.Value, roleId, request.RoleName);

        return Result.Success<bool>(true);
    }

    public async Task<Result<bool>> AssignPermissionsAsync(int roleId, AssignPermissionsRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);

        if (role == null) return Result.Failure<bool>("RoleNotFound");

        // Simple validation: ensure permissions exist
        var existingPermIds = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.PermissionId))
            .Select(p => p.PermissionId)
            .ToListAsync();

        if (existingPermIds.Count != request.PermissionIds.Count)
        {
            return Result.Failure<bool>("One or more permissions are invalid");
        }

        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) return Result.Failure<bool>("Unauthorized");

        bool isCallerSuperAdmin = await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == currentUserId.Value && ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        // Protected-role check: only the System Administrator may modify built-in
        // administrator roles (Administrator and System Administrator). Other users
        // with Permission.Assign may delegate permissions only to lower-level roles.
        if ((role.RoleName == AppConstants.SystemAdministratorRole || role.RoleName == AppConstants.AdministratorRole) && !isCallerSuperAdmin)
        {
            return Result.Failure<bool>("ProtectedRole: Only the System Administrator can modify built-in administrator roles.");
        }

        // Delegation guard: a non-System-Administrator caller may only delegate
        // permissions that fall within their own authority (prevents privilege escalation).
        if (!isCallerSuperAdmin)
        {
            var callerPermissionIds = await _context.UserRoles
                .Where(ur => ur.UserId == currentUserId.Value)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToListAsync();

            var unauthorizedPermissionIds = request.PermissionIds.Except(callerPermissionIds).ToList();
            if (unauthorizedPermissionIds.Any())
            {
                return Result.Failure<bool>("PrivilegeEscalation: You cannot delegate permissions that you do not possess.");
            }
        }

        // Lockout protection: the System Administrator role's permissions can never be modified.
        if (role.RoleName == AppConstants.SystemAdministratorRole)
        {
            return Result.Failure<bool>("Cannot modify permissions of System Administrator role to prevent lockout");
        }

        // Remove old
        _context.RolePermissions.RemoveRange(role.RolePermissions);
        
        // Add new
        foreach (var pId in request.PermissionIds)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = pId
            });
        }

        await _context.SaveChangesAsync();
        _permissionService.InvalidatePermissionCache();

        _logger.LogInformation("User {UserId} updated permissions for Role {RoleId} (Assigned {PermissionCount} permissions)", currentUserId.Value, roleId, request.PermissionIds.Count);

        return Result.Success<bool>(true);
    }
}
