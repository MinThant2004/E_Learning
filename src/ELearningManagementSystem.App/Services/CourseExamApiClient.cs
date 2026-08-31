using System.Net.Http.Json;

namespace ELearningManagementSystem.App.Services;

public class CourseExamApiClient
{
    private readonly HttpClient _httpClient;

    public CourseExamApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<CourseExamResponse>?> GetExamsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isArchived = false, int? courseId = null)
    {
        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (isArchived.HasValue) queryParams += $"&isArchived={isArchived.Value.ToString().ToLower()}";
        if (courseId.HasValue) queryParams += $"&courseId={courseId.Value}";

        return await _httpClient.GetFromJsonAsync<PagedResult<CourseExamResponse>>($"api/course-exams{queryParams}");
    }

    public async Task<CourseExamResponse?> GetExamByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<CourseExamResponse>($"api/course-exams/{id}");
    }

    public async Task<HttpResponseMessage> CreateExamAsync(CreateCourseExamRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/course-exams", request);
    }

    public async Task<HttpResponseMessage> UpdateExamAsync(int id, UpdateCourseExamRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/course-exams/{id}", request);
    }

    public async Task<HttpResponseMessage> ArchiveExamAsync(int id)
    {
        return await _httpClient.PostAsync($"api/course-exams/{id}/archive", null);
    }

    public async Task<HttpResponseMessage> RestoreExamAsync(int id)
    {
        return await _httpClient.PostAsync($"api/course-exams/{id}/restore", null);
    }

    public async Task<List<ExamQuestionResponse>?> GetQuestionPoolAsync(int examId)
    {
        return await _httpClient.GetFromJsonAsync<List<ExamQuestionResponse>>($"api/course-exams/{examId}/questions");
    }

    public async Task<HttpResponseMessage> AddQuestionToPoolAsync(int examId, CreateExamQuestionRequest request)
    {
        return await _httpClient.PostAsJsonAsync($"api/course-exams/{examId}/questions", request);
    }

    public async Task<HttpResponseMessage> UpdateQuestionInPoolAsync(int questionId, UpdateExamQuestionRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/exam-questions/{questionId}", request);
    }

    public async Task<HttpResponseMessage> DeleteQuestionFromPoolAsync(int questionId)
    {
        return await _httpClient.DeleteAsync($"api/exam-questions/{questionId}");
    }
}

public class CourseExamResponse
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public int PoolQuestionCount { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
}

public class CreateCourseExamRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
}

public class UpdateCourseExamRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool Status { get; set; } = true;
}

public class ExamQuestionResponse
{
    public int ExamQuestionId { get; set; }
    public int ExamId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string DifficultyLevel { get; set; } = "Medium";
    public List<ExamQuestionOptionResponse> Options { get; set; } = new();
}

public class ExamQuestionOptionResponse
{
    public int OptionId { get; set; }
    public int ExamQuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Medium";
    public List<CreateExamOptionRequest> Options { get; set; } = new();
}

public class CreateExamOptionRequest
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class UpdateExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Medium";
    public List<CreateExamOptionRequest> Options { get; set; } = new();
}
