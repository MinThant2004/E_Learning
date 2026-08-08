using Microsoft.AspNetCore.Authorization;

namespace ELearningManagementSystem.Infrastructure.Security.Attributes;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public const string Prefix = "Permission:";

    public HasPermissionAttribute(string permission)
    {
        Policy = $"{Prefix}{permission}";
    }
}
