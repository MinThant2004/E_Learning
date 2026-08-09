using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Application.Features.Users.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Domain.Entities;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UserService> _logger;

    public UserService(IAppDbContext context, IAuditLogService auditLogService, ILogger<UserService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<PagedResult<AdminUserSummaryResponse>> GetPagedUsersAsync(UserListQuery query)
    {
        var dbQuery = _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .AsNoTracking();

        if (query.ArchiveFilter == UserArchiveFilter.Active)
        {
            dbQuery = dbQuery.Where(u => !u.DeleteFlag);
        }
        else if (query.ArchiveFilter == UserArchiveFilter.Archived)
        {
            dbQuery = dbQuery.Where(u => u.DeleteFlag);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(u => u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.RoleFilter) && query.RoleFilter != "All Roles")
        {
            dbQuery = dbQuery.Where(u => u.UserRoleUsers.Any(ur => ur.Role.RoleName == query.RoleFilter));
        }

        var totalCount = await dbQuery.CountAsync();

        var users = await dbQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new AdminUserSummaryResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Status = u.Status,
                IsArchived = u.DeleteFlag,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList()
            })
            .ToListAsync();

        return new PagedResult<AdminUserSummaryResponse>(users, totalCount, query.Page, query.PageSize);
    }

    public async Task<Result<AdminUserDetailResponse>> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.AssignedByNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Result.Failure<AdminUserDetailResponse>("UserNotFound: The user was not found.");

        var detail = new AdminUserDetailResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Status = user.Status,
            IsArchived = user.DeleteFlag,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = user.UserRoleUsers.Select(ur => new UserRoleSummaryResponse
            {
                RoleName = ur.Role.RoleName,
                AssignedAt = ur.AssignedAt,
                AssignedBy = ur.AssignedByNavigation != null ? ur.AssignedByNavigation.FullName : "System"
            }).ToList()
        };

        return Result.Success(detail);
    }

    public async Task<Result<bool>> UpdateUserAsync(int userId, UpdateUserRequest request, int currentUserId)
    {
        var currentUser = await _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
             return Result.Failure<bool>("Unauthorized: Current user not found.");
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");

        var user = await _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (user.DeleteFlag)
            return Result.Failure<bool>("UserArchived: Cannot update an archived user.");

        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin");
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a Super Admin can modify another Administrator.");

        user.FullName = request.FullName;
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        if (request.Roles != null && request.Roles.Any())
        {
            if (request.Roles.Contains("SuperAdmin") && !isCurrentUserSuperAdmin)
            {
                return Result.Failure<bool>("PrivilegeEscalation: You cannot assign the SuperAdmin role.");
            }

            if (!isCurrentUserSuperAdmin)
            {
                // Get all permissions of the current user
                var currentUserPermissionIds = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.PermissionId)
                    .Distinct()
                    .ToListAsync();

                // Get all permissions of the roles being assigned
                var rolesBeingAssignedPermissionIds = await _context.Roles
                    .Where(r => request.Roles.Contains(r.RoleName))
                    .SelectMany(r => r.RolePermissions)
                    .Select(rp => rp.PermissionId)
                    .Distinct()
                    .ToListAsync();

                var unauthorizedPermissionIds = rolesBeingAssignedPermissionIds.Except(currentUserPermissionIds).ToList();
                if (unauthorizedPermissionIds.Any())
                {
                    return Result.Failure<bool>("PrivilegeEscalation: You cannot assign a role with permissions that exceed your own.");
                }
            }

            var dbRoles = await _context.Roles.Where(r => request.Roles.Contains(r.RoleName)).ToListAsync();
            if (dbRoles.Count != request.Roles.Distinct().Count())
            {
                return Result.Failure<bool>("RoleNotFound: One or more selected roles no longer exist.");
            }
            
            // Prevent removing all admins
            var currentAdminRoles = user.UserRoleUsers.Where(ur => ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin").ToList();
            if (currentAdminRoles.Any() && !dbRoles.Any(r => r.RoleName == "Admin" || r.RoleName == "SuperAdmin"))
            {
                var adminRoleCount = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Include(ur => ur.User)
                    .Where(ur => (ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin") && !ur.User.DeleteFlag)
                    .CountAsync();

                if (adminRoleCount <= 1)
                {
                    return Result.Failure<bool>("CannotRemoveLastAdmin: Cannot remove the Admin role from the last active administrative user.");
                }
            }

            if (isTargetSuperAdmin && !dbRoles.Any(r => r.RoleName == "SuperAdmin"))
            {
                var superAdminRoleCount = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Include(ur => ur.User)
                    .Where(ur => ur.Role.RoleName == "SuperAdmin" && !ur.User.DeleteFlag)
                    .CountAsync();

                if (superAdminRoleCount <= 1)
                {
                    return Result.Failure<bool>("CannotRemoveLastSuperAdmin: Cannot remove the SuperAdmin role from the last active Super Admin.");
                }
            }

            // Remove unselected roles
            var rolesToRemove = user.UserRoleUsers.Where(ur => !request.Roles.Contains(ur.Role.RoleName)).ToList();
            foreach (var r in rolesToRemove)
            {
                _context.UserRoles.Remove(r);
            }

            // Add new roles
            foreach (var r in dbRoles)
            {
                if (!user.UserRoleUsers.Any(ur => ur.RoleId == r.RoleId))
                {
                    user.UserRoleUsers.Add(new UserRole
                    {
                        RoleId = r.RoleId,
                        UserId = user.UserId,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync(default);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = currentUserId,
            Action = "Update",
            TableName = "Users",
            RecordId = user.UserId
        });
        
        _logger.LogInformation("User {CurrentUserId} updated User {TargetUserId} (Assigned {RoleCount} roles)", currentUserId, userId, request.Roles?.Count ?? 0);

        return Result.Success(true);
    }

    public async Task<Result<bool>> ArchiveUserAsync(int userId, int currentUserId)
    {
        if (userId == currentUserId)
            return Result.Failure<bool>("CannotArchiveSelf: You cannot archive your own account.");

        var currentUser = await _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
             return Result.Failure<bool>("Unauthorized: Current user not found.");
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");

        var user = await _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (user.DeleteFlag)
            return Result.Failure<bool>("UserAlreadyArchived: The user is already archived.");

        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin");
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a Super Admin can deactivate another Administrator.");

        if (isTargetSuperAdmin)
        {
            var superAdminRoleCount = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => ur.Role.RoleName == "SuperAdmin" && !ur.User.DeleteFlag)
                .CountAsync();

            if (superAdminRoleCount <= 1)
            {
                return Result.Failure<bool>("CannotArchiveLastSuperAdmin: Cannot deactivate the last active Super Admin.");
            }
        }

        bool isAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin");
        if (isAdmin)
        {
            var adminRoleCount = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => (ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin") && !ur.User.DeleteFlag)
                .CountAsync();

            if (adminRoleCount <= 1)
            {
                return Result.Failure<bool>("CannotArchiveLastAdmin: Cannot archive the last active administrative user.");
            }
        }

        user.DeleteFlag = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        // Revoke active refresh tokens
        var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow).ToListAsync();
        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(default);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = currentUserId,
            Action = "Archive",
            TableName = "Users",
            RecordId = user.UserId
        });

        _logger.LogInformation("User {CurrentUserId} archived User {TargetUserId}", currentUserId, userId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RestoreUserAsync(int userId, int currentUserId)
    {
        var currentUser = await _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
             return Result.Failure<bool>("Unauthorized: Current user not found.");
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");

        var user = await _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (!user.DeleteFlag)
            return Result.Failure<bool>("UserNotArchived: The user is not archived.");
            
        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "SuperAdmin");
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == "Admin" || ur.Role.RoleName == "SuperAdmin");
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a Super Admin can activate another Administrator.");

        user.DeleteFlag = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = currentUserId,
            Action = "Restore",
            TableName = "Users",
            RecordId = user.UserId
        });

        _logger.LogInformation("User {CurrentUserId} restored User {TargetUserId}", currentUserId, userId);

        return Result.Success(true);
    }

    public async Task<Result<List<string>>> GetAvailableRolesAsync()
    {
        var roles = await _context.Roles.Select(r => r.RoleName).ToListAsync();
        return Result.Success(roles);
    }
}
