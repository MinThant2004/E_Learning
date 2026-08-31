using System;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class CoursePerformanceReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "TotalEnrollments";
    public bool SortDescending { get; set; } = true;
}

public class CoursePerformanceReportDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool CourseStatus { get; set; }
    public int TotalEnrollments { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int NotStartedCount { get; set; }
    public double CompletionRatePercentage { get; set; }
    public int TotalLessonsCount { get; set; }
    public int TotalQuizzesCount { get; set; }
}
