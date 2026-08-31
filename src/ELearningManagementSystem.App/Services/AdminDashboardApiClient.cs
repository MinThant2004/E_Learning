using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ELearningManagementSystem.App.Services
{
    public class AdminDashboardApiClient
    {
        private readonly HttpClient _httpClient;

        public AdminDashboardApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, AdminDashboardResponse? Data, string? Error)> GetDashboardAsync(string range = "monthly", DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = $"api/admin/dashboard?range={range}";
            if (range == "custom" && startDate.HasValue && endDate.HasValue)
            {
                url += $"&startDate={startDate.Value:yyyy-MM-dd}&endDate={endDate.Value:yyyy-MM-dd}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AdminDashboardResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, data, null);
            }
            
            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, null, error);
        }
    }

    public class AdminDashboardResponse
    {
        public Dictionary<string, ContentMetricResponse> Metrics { get; set; } = new();
        public List<AdminQuickActionResponse> QuickActions { get; set; } = new();

        public EnrollmentAnalyticsResponse? EnrollmentAnalytics { get; set; }
        public CourseCompletionStatsDto? CompletionStats { get; set; }
        public ExamPerformanceStatsDto? ExamPerformance { get; set; }
        public ExamPaymentStatsDto? ExamPaymentStats { get; set; }

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

    public class EnrollmentTrendItemDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class EnrollmentAnalyticsResponse
    {
        public string Range { get; set; } = "monthly";
        public List<EnrollmentTrendItemDto> Trends { get; set; } = new();
    }

    public class CourseCompletionStatsDto
    {
        public int TotalEnrollments { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int NotCompletedCount { get; set; }
        public double CompletionRatePercentage { get; set; }
    }

    public class ExamPerformanceStatsDto
    {
        public int TotalExamAttempts { get; set; }
        public int PassedExamAttempts { get; set; }
        public int FailedExamAttempts { get; set; }
        public double ExamAverageScore { get; set; }
        public double ExamPassRate { get; set; }
        public double ExamFailRate { get; set; }
    }

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
        public string IconType { get; set; } = "info";
        public DateTime CreatedAt { get; set; }
    }

    public class ExamPaymentStatsDto
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
