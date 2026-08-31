using System;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.AdminDashboard.DTOs
{
    public class EnrollmentTrendItemDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class EnrollmentAnalyticsResponse
    {
        public string Range { get; set; } = "monthly"; // "daily", "weekly", "monthly", "custom"
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
}
