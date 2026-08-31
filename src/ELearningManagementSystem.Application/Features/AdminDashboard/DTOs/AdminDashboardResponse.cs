using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.AdminDashboard.DTOs
{
    public class AdminDashboardResponse
    {
        public Dictionary<string, ContentMetricResponse> Metrics { get; set; } = new();
        public List<AdminQuickActionResponse> QuickActions { get; set; } = new();

        public EnrollmentAnalyticsResponse? EnrollmentAnalytics { get; set; }
        public CourseCompletionStatsDto? CompletionStats { get; set; }
        public QuizPerformanceStatsDto? QuizPerformance { get; set; }

        public List<PopularCourseSummaryDto> PopularCourses { get; set; } = new();
        public List<RecentActivityItemDto> RecentActivities { get; set; } = new();
    }

    public class ContentMetricResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ActiveCount { get; set; }
        public int ArchivedCount { get; set; }
        public int TotalCount => ActiveCount + ArchivedCount;
        public string Route { get; set; } = string.Empty;
    }

    public class AdminQuickActionResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
