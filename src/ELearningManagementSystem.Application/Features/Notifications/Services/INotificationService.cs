using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Notifications.DTOs;

namespace ELearningManagementSystem.Application.Features.Notifications.Services;

public interface INotificationService
{
    Task<Result<List<UserNotificationResponse>>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<List<UserNotificationResponse>>> GetAdminNotificationsAsync(int adminUserId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);
}
