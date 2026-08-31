using System;

namespace ELearningManagementSystem.Application.Features.AdminDashboard.DTOs
{
    public class PopularCourseSummaryDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int EnrolledStudentsCount { get; set; }
        public double CompletionRatePercentage { get; set; }
    }

    public class RecentActivityItemDto
    {
        public int AuditLogId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string RelativeTime { get; set; } = string.Empty;
        public string IconType { get; set; } = "info"; // "create", "update", "delete", "role", "info"
        public DateTime CreatedAt { get; set; }
    }
}
