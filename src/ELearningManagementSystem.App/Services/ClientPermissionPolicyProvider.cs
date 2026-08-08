using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ELearningManagementSystem.App.Services;

/// <summary>
/// Client-side IAuthorizationPolicyProvider for Blazor WASM.
/// Dynamically creates authorization policies for permission-based checks
/// matching the "Permission:" prefix convention used by the API's HasPermissionAttribute.
/// The policy requires that the user has a "permission" claim with the required value.
/// </summary>
public class ClientPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "Permission:";
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public ClientPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PermissionPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("permission", permission)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();
}
