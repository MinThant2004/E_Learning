using System.Security.Claims;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ELearningManagementSystem.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string? FullName => User?.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
