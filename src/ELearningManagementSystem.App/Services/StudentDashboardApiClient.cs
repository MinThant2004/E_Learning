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
        public List<StudentCourseDashboardItem> ActiveCourses { get; set; } = new();
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
    }

    public class StudentQuizSummaryResponse
    {
        public int AttemptId { get; set; }
        public double Score { get; set; }
        public bool Passed { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
