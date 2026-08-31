namespace ELearningManagementSystem.Domain.Entities;

public partial class UserNotification
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; } // Null if targeted broadly at all Admins
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "Info"; // PaymentSubmitted, PaymentApproved, PaymentRejected
    public string TargetUrl { get; set; } = "/course-exams";
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool DeleteFlag { get; set; } = false;

    public virtual User? User { get; set; }
}
