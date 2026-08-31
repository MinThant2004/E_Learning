namespace ELearningManagementSystem.Application.Features.Notifications.DTOs;

public class UserNotificationResponse
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string TargetUrl { get; set; } = "/course-exams";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = "Just now";
}

public class CreateNotificationRequest
{
    public int? UserId { get; set; } // Null if targeted broadly at Admins
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string TargetUrl { get; set; } = "/course-exams";
}
