using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services
{
    public class StudentDashboardApiClient
    {
        private readonly HttpClient _httpClient;

        public StudentDashboardApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, StudentDashboardResponse? Data, string? Error)> GetDashboardSummaryAsync()
        {
            var response = await _httpClient.GetAsync("api/student/dashboard");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<StudentDashboardResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, data, null);
            }

            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, null, error);
        }
    }

    public class StudentDashboardResponse
    {
        public StudentCourseDashboardItem? ContinueLearningCourse { get; set; }
        public List<StudentCourseDashboardItem> CoursesInProgress { get; set; } = new();
        public List<StudentCompletedCourseItem> CompletedCourses { get; set; } = new();
        public StudentLearningOverviewResponse Overview { get; set; } = new();
        public List<StudentQuizResultItem> RecentQuizResults { get; set; } = new();
        public List<StudentActivityItem> RecentActivity { get; set; } = new();
        public int LessonsThisWeek { get; set; }
        public List<StudentDayActivityItem> WeekActivity { get; set; } = new();
    }

    public class StudentCompletedCourseItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime? CompletedDate { get; set; }
        public double? FinalScore { get; set; }
    }

    public class StudentDayActivityItem
    {
        public DateTime Date { get; set; }
        public int LessonsCompleted { get; set; }
    }

    public class StudentCourseDashboardItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }

        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int ProgressPercentage => TotalLessons == 0 ? 0 : (int)Math.Round((double)CompletedLessons / TotalLessons * 100);
        public bool IsCompleted => TotalLessons > 0 && CompletedLessons == TotalLessons;

        public int? NextLessonId { get; set; }
        public string? NextLessonTitle { get; set; }

        public bool IsFinalQuizAvailable { get; set; }
        public StudentQuizSummaryResponse? LatestQuizAttempt { get; set; }

        public DateTime LastActivityAt { get; set; }
    }

    public class StudentLearningOverviewResponse
    {
        public int TotalEnrolledCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int CompletedLessons { get; set; }
        public int QuizAttemptsCount { get; set; }
        public int QuizzesPassed { get; set; }
        public double AverageQuizScore { get; set; }
        public bool HasQuizData => QuizAttemptsCount > 0;
    }

    public class StudentQuizSummaryResponse
    {
        public int AttemptId { get; set; }
        public double Score { get; set; }
        public bool Passed { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class StudentQuizResultItem
    {
        public int AttemptId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public double Score { get; set; }
        public bool Passed { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class StudentActivityItem
    {
        // "Enrolled" | "LessonCompleted"
        public string ActivityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int? LessonId { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
