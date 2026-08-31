using ELearningManagementSystem.Shared.Constants;
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
    private readonly IPermissionService _permissionService;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IAppDbContext context, 
        IAuditLogService auditLogService, 
        ILogger<UserService> logger, 
        IPermissionService permissionService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
        _permissionService = permissionService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<int>> CreateUserAsync(CreateUserRequest request, int currentUserId)
    {
        try
        {
            var currentUser = await _context.Users
                .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (currentUser == null)
                 return Result.Failure<int>("Unauthorized: Current user not found.");

            bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);

            // Privilege Escalation Guard
            if (request.Roles.Contains(AppConstants.SystemAdministratorRole) && !isCurrentUserSuperAdmin)
            {
                return Result.Failure<int>("PrivilegeEscalation: You cannot assign the SuperAdmin role.");
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
                    return Result.Failure<int>("PrivilegeEscalation: You cannot assign a role with permissions that exceed your own.");
                }
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Check duplicate email
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (existingUser)
            {
                return Result.Failure<int>("EmailAlreadyExists: An account with this email already exists.");
            }

            var dbRoles = await _context.Roles.Where(r => request.Roles.Contains(r.RoleName)).ToListAsync();
            if (dbRoles.Count != request.Roles.Distinct().Count())
            {
                return Result.Failure<int>("RoleNotFound: One or more selected roles do not exist.");
            }

            var passwordHash = _passwordHasher.Hash(request.Password);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Status = true,
                MustChangePassword = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(default);

            foreach (var r in dbRoles)
            {
                _context.UserRoles.Add(new UserRole
                {
                    RoleId = r.RoleId,
                    UserId = user.UserId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = currentUserId
                });
            }

            await _context.SaveChangesAsync(default);

            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = currentUserId,
                Action = "Create",
                TableName = "Users",
                RecordId = user.UserId
            });

            _logger.LogInformation("User {CurrentUserId} created User {TargetUserId} with roles {Roles}", currentUserId, user.UserId, string.Join(", ", request.Roles));

            return Result.Success(user.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateUserAsync");
            throw;
        }
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
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        var user = await _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (user.DeleteFlag)
            return Result.Failure<bool>("UserArchived: Cannot update an archived user.");

        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a System Administrator can modify another Administrator.");

        var oldFullName = user.FullName;
        var oldStatus = user.Status;
        var oldRoleNames = string.Join(", ", user.UserRoleUsers.Select(ur => ur.Role.RoleName).OrderBy(r => r, StringComparer.OrdinalIgnoreCase));

        user.FullName = request.FullName;
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        if (request.Roles != null && request.Roles.Any())
        {
            if (request.Roles.Contains(AppConstants.SystemAdministratorRole) && !isCurrentUserSuperAdmin)
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
            var currentAdminRoles = user.UserRoleUsers.Where(ur => ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole).ToList();
            if (currentAdminRoles.Any() && !dbRoles.Any(r => r.RoleName == AppConstants.AdministratorRole || r.RoleName == AppConstants.SystemAdministratorRole))
            {
                var adminRoleCount = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Include(ur => ur.User)
                    .Where(ur => (ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole) && !ur.User.DeleteFlag)
                    .CountAsync();

                if (adminRoleCount <= 1)
                {
                    return Result.Failure<bool>("CannotRemoveLastAdmin: Cannot remove the Admin role from the last active administrative user.");
                }
            }

            if (isTargetSuperAdmin && !dbRoles.Any(r => r.RoleName == AppConstants.SystemAdministratorRole))
            {
                var superAdminRoleCount = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Include(ur => ur.User)
                    .Where(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole && !ur.User.DeleteFlag)
                    .CountAsync();

                if (superAdminRoleCount <= 1)
                {
                    return Result.Failure<bool>("CannotRemoveLastSuperAdmin: Cannot remove the System Administrator role from the last active System Administrator.");
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
        _permissionService.InvalidatePermissionCache();

        var changes = new List<AuditLogChangeDto>();
        if (!string.Equals(oldFullName ?? string.Empty, user.FullName ?? string.Empty, StringComparison.Ordinal))
            changes.Add(new AuditLogChangeDto { Field = "Full Name", OldValue = oldFullName, NewValue = user.FullName });
        if (oldStatus != user.Status)
            changes.Add(new AuditLogChangeDto { Field = "Status", OldValue = FormatUserStatus(oldStatus), NewValue = FormatUserStatus(user.Status) });

        var newRoleNames = string.Join(", ", request.Roles != null ? request.Roles.OrderBy(r => r, StringComparer.OrdinalIgnoreCase) : Enumerable.Empty<string>());
        if (!string.Equals(oldRoleNames, newRoleNames, StringComparison.OrdinalIgnoreCase))
            changes.Add(new AuditLogChangeDto { Field = "Roles", OldValue = oldRoleNames, NewValue = newRoleNames });

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = currentUserId,
            Action = "Update",
            TableName = "Users",
            RecordId = user.UserId,
            Changes = changes
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
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        var user = await _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (user.DeleteFlag)
            return Result.Failure<bool>("UserAlreadyArchived: The user is already archived.");

        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a System Administrator can deactivate another Administrator.");

        if (isTargetSuperAdmin)
        {
            var superAdminRoleCount = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole && !ur.User.DeleteFlag)
                .CountAsync();

            if (superAdminRoleCount <= 1)
            {
                return Result.Failure<bool>("CannotArchiveLastSuperAdmin: Cannot deactivate the last active System Administrator.");
            }
        }

        bool isAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        if (isAdmin)
        {
            var adminRoleCount = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => (ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole) && !ur.User.DeleteFlag)
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
        _permissionService.InvalidatePermissionCache();

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
             
        bool isCurrentUserSuperAdmin = currentUser.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);

        var user = await _context.Users
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null)
            return Result.Failure<bool>("UserNotFound: The user was not found.");

        if (!user.DeleteFlag)
            return Result.Failure<bool>("UserNotArchived: The user is not archived.");
            
        bool isTargetSuperAdmin = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        bool isTargetAdminOrSuper = user.UserRoleUsers.Any(ur => ur.Role.RoleName == AppConstants.AdministratorRole || ur.Role.RoleName == AppConstants.SystemAdministratorRole);
        if (isTargetAdminOrSuper && !isCurrentUserSuperAdmin && user.UserId != currentUserId)
            return Result.Failure<bool>("ProtectedAccount: Only a System Administrator can activate another Administrator.");

        user.DeleteFlag = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);
        _permissionService.InvalidatePermissionCache();

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

    public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return Result.Failure<bool>("UserNotFound: The user was not found.");

            if (user.DeleteFlag)
                return Result.Failure<bool>("UserArchived: Cannot change password of an archived account.");

            // Verify the current password
            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                return Result.Failure<bool>("InvalidCurrentPassword: The current password is incorrect.");

            // Hash and persist the new password
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(default);

            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = userId,
                Action = "ChangePassword",
                TableName = "Users",
                RecordId = userId
            });

            _logger.LogInformation("User {UserId} changed their password", userId);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ChangePasswordAsync for User {UserId}", userId);
            throw;
        }
    }

    public async Task<Result<List<string>>> GetAvailableRolesAsync()
    {
        var roles = await _context.Roles.Select(r => r.RoleName).ToListAsync();
        return Result.Success(roles);
    }

    private static string FormatUserStatus(bool status) => status ? "Active" : "Inactive";
}
