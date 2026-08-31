using System.Net.Http.Json;
using System.Text.Json;

namespace ELearningManagementSystem.App.Services;

public class QuizAttemptApiClient
{
    private readonly HttpClient _httpClient;

    public QuizAttemptApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, AvailableQuizResponse? Data, string? Error)> GetAvailableQuizAsync(int courseId)
    {
        var response = await _httpClient.GetAsync($"api/courses/{courseId}/final-quiz");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<AvailableQuizResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<(bool Success, QuizAttemptResultResponse? Data, string? Error)> SubmitQuizAttemptAsync(int courseId, SubmitQuizAttemptRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/courses/{courseId}/final-quiz/attempts", request);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<QuizAttemptResultResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<(bool Success, List<QuizAttemptHistoryResponse>? Data, string? Error)> GetStudentAttemptHistoryAsync(int courseId)
    {
        var response = await _httpClient.GetAsync($"api/courses/{courseId}/final-quiz/attempts");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<List<QuizAttemptHistoryResponse>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<(bool Success, QuizAttemptResultResponse? Data, string? Error)> GetAttemptDetailsAsync(int attemptId)
    {
        var response = await _httpClient.GetAsync($"api/quiz-attempts/{attemptId}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<QuizAttemptResultResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, null, error);
    }
}

public class AvailableQuizResponse
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<StudentQuizQuestionResponse> Questions { get; set; } = new();
}

public class StudentQuizQuestionResponse
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<StudentQuizOptionResponse> Options { get; set; } = new();
}

public class StudentQuizOptionResponse
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class SubmitQuizAttemptRequest
{
    public List<SubmitQuizAnswerRequest> Answers { get; set; } = new();
}

public class SubmitQuizAnswerRequest
{
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
}

public class QuizAttemptResultResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class QuizAttemptHistoryResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
}
