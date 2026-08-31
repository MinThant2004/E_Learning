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

    public class QuizPerformanceStatsDto
    {
        public int TotalAttempts { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public double AverageScore { get; set; }
        public double PassRatePercentage { get; set; }
        public double FailRatePercentage { get; set; }
        public double HighestScore { get; set; }
        public int TotalQuizzes { get; set; }
        public string StatusHealth { get; set; } = "Good";
    }
}
