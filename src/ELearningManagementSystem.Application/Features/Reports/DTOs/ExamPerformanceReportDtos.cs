using System;
using ELearningManagementSystem.Application.Common;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class ExamPerformanceReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "TotalAttempts";
    public bool SortDescending { get; set; } = true;
}

public class ExamPerformanceReportDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public decimal ExamFee { get; set; }

    public int TotalAttempts { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public double AverageScore { get; set; }
    public double PassRatePercentage { get; set; }
    public double FailRatePercentage { get; set; }
    public double HighestScore { get; set; }
}

public class ExamPerformanceReportSummaryDto
{
    public int TotalAttempts { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public double AverageScore { get; set; }
    public double PassRatePercentage { get; set; }
}

public class ExamPerformanceReportResponse
{
    public PagedResult<ExamPerformanceReportDto> Data { get; set; } = null!;
    public ExamPerformanceReportSummaryDto Summary { get; set; } = new();
}
