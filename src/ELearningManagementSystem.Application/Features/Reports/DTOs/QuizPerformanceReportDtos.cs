using System;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class QuizPerformanceReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "TotalAttempts";
    public bool SortDescending { get; set; } = true;
}

public class QuizPerformanceReportDto
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal PassingScore { get; set; }
    public int TotalAttempts { get; set; }
    public int PassedAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double AverageScore { get; set; }
    public double PassRatePercentage { get; set; }
    public double FailRatePercentage { get; set; }
    public double HighestScore { get; set; }
}
