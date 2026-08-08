using System;

namespace ELearningManagementSystem.Application.Features.Enrollments.DTOs;

public class EnrollCourseRequest
{
}

public class EnrollmentSummaryResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime EnrollDate { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class MyCourseResponse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int EnrollmentId { get; set; }
    public DateTime EnrollDate { get; set; }
    public bool Completed { get; set; }
    public int CompletedCount { get; set; }
    public int TotalActiveLessons { get; set; }
    public double ProgressPercentage => TotalActiveLessons == 0 ? 0 : Math.Round((double)CompletedCount / TotalActiveLessons * 100, 2);
    public bool IsComplete => TotalActiveLessons > 0 && CompletedCount >= TotalActiveLessons;
}
