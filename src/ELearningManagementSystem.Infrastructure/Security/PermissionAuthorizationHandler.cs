using System.Security.Claims;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Infrastructure.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(IPermissionService permissionService, ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Authorization failed: user is not authenticated.");
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Authorization failed: valid user ID claim not found in JWT.");
            return;
        }

        if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
        {
            _logger.LogInformation("Authorization succeeded: UserId={UserId} has permission={Permission}", userId, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed: UserId={UserId} lacks permission={Permission}", userId, requirement.Permission);
        }
    }
}
