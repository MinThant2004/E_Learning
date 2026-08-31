using ELearningManagementSystem.Application.Features.Notifications.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    /// <summary>GET /api/notifications — Get current user / admin notifications</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("System Administrator") || User.IsInRole("Administrator") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Course.Update");

        if (isAdmin)
        {
            var adminResult = await _notificationService.GetAdminNotificationsAsync(userId.Value, cancellationToken);
            return adminResult.ToActionResult();
        }

        var userResult = await _notificationService.GetUserNotificationsAsync(userId.Value, cancellationToken);
        return userResult.ToActionResult();
    }

    /// <summary>POST /api/notifications/{id:int}/read — Mark notification as read</summary>
    [HttpPost("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var result = await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
        return result.ToActionResult();
    }
}
