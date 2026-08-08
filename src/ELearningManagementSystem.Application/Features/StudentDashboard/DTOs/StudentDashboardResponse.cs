namespace ELearningManagementSystem.Application.Features.StudentDashboard.DTOs
{
    public class StudentDashboardResponse
    {
        public List<StudentCourseDashboardItem> ActiveCourses { get; set; } = new();
    }

    public class StudentCourseDashboardItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        
        // Progress
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int ProgressPercentage => TotalLessons == 0 ? 0 : (int)Math.Round((double)CompletedLessons / TotalLessons * 100);
        public bool IsCompleted => TotalLessons > 0 && CompletedLessons == TotalLessons;

        // Next Action
        public int? NextLessonId { get; set; }
        public string? NextLessonTitle { get; set; }

        // Final Quiz State
        public bool IsFinalQuizAvailable { get; set; }
        public StudentQuizSummaryResponse? LatestQuizAttempt { get; set; }
    }

    public class StudentQuizSummaryResponse
    {
        public int AttemptId { get; set; }
        public double Score { get; set; }
        public bool Passed { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
