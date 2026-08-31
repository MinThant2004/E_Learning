using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Notifications.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _context;

    public NotificationService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserNotificationResponse>>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var list = await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.DeleteFlag)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new UserNotificationResponse
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                TargetUrl = n.TargetUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        return Result.Success(list);
    }

    public async Task<Result<List<UserNotificationResponse>>> GetAdminNotificationsAsync(int adminUserId, CancellationToken cancellationToken = default)
    {
        var list = await _context.UserNotifications
            .Where(n => (n.UserId == null || n.UserId == adminUserId) && !n.DeleteFlag)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new UserNotificationResponse
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                TargetUrl = n.TargetUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        return Result.Success(list);
    }

    public async Task<Result<bool>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new UserNotification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            TargetUrl = request.TargetUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.UserNotifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserNotifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);
        if (entity != null)
        {
            entity.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return Result.Success(true);
    }

    private static string GetTimeAgo(DateTime dt)
    {
        var ts = DateTime.UtcNow - dt;
        if (ts.TotalMinutes < 1) return "Just now";
        if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m ago";
        if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h ago";
        return $"{(int)ts.TotalDays}d ago";
    }
}
