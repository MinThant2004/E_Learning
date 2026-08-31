using System;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class EnrollmentReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; } // "all", "completed", "in_progress", "not_started"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "EnrollDate";
    public bool SortDescending { get; set; } = true;
}

public class EnrollmentReportDto
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string CompletionStatus { get; set; } = string.Empty; // "Completed", "In Progress", "Not Started"
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
    public double ProgressPercentage { get; set; }
}
