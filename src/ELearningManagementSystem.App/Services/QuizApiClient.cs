using System.Net.Http;
using System.Net.Http.Json;

namespace ELearningManagementSystem.App.Services;

public class QuizApiClient
{
    private readonly HttpClient _httpClient;

    public QuizApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ─── Quiz ────────────────────────────────────────────────────

    public async Task<List<QuizSummaryResponse>?> GetQuizzesByCourseAsync(int courseId, bool? isArchived = null)
    {
        var url = $"api/courses/{courseId}/quizzes";
        if (isArchived.HasValue)
        {
            url += $"?isArchived={isArchived.Value.ToString().ToLower()}";
        }
        return await _httpClient.GetFromJsonAsync<List<QuizSummaryResponse>>(url);
    }

    public async Task<QuizDetailClientResponse?> GetQuizAsync(int quizId)
    {
        return await _httpClient.GetFromJsonAsync<QuizDetailClientResponse>($"api/quizzes/{quizId}");
    }

    public async Task<HttpResponseMessage> CreateQuizAsync(int courseId, CreateQuizClientRequest request)
    {
        return await _httpClient.PostAsJsonAsync($"api/courses/{courseId}/quizzes", request);
    }

    public async Task<HttpResponseMessage> UpdateQuizAsync(int quizId, UpdateQuizClientRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/quizzes/{quizId}", request);
    }

    public async Task<HttpResponseMessage> ArchiveQuizAsync(int quizId)
    {
        return await _httpClient.PostAsync($"api/quizzes/{quizId}/archive", null);
    }

    public async Task<HttpResponseMessage> RestoreQuizAsync(int quizId)
    {
        return await _httpClient.PostAsync($"api/quizzes/{quizId}/restore", null);
    }

    // ─── Questions ───────────────────────────────────────────────

    public async Task<HttpResponseMessage> CreateQuestionAsync(int quizId, CreateQuestionClientRequest request)
    {
        return await _httpClient.PostAsJsonAsync($"api/quizzes/{quizId}/questions", request);
    }

    public async Task<HttpResponseMessage> UpdateQuestionAsync(int questionId, UpdateQuestionClientRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/questions/{questionId}", request);
    }

    public async Task<HttpResponseMessage> DeleteQuestionAsync(int questionId)
    {
        return await _httpClient.DeleteAsync($"api/questions/{questionId}");
    }

    // ─── Options ─────────────────────────────────────────────────

    public async Task<List<QuestionOptionClientResponse>?> GetOptionsAsync(int questionId)
    {
        return await _httpClient.GetFromJsonAsync<List<QuestionOptionClientResponse>>($"api/questions/{questionId}/options");
    }

    public async Task<HttpResponseMessage> CreateOptionAsync(int questionId, CreateOptionClientRequest request)
    {
        return await _httpClient.PostAsJsonAsync($"api/questions/{questionId}/options", request);
    }

    public async Task<HttpResponseMessage> UpdateOptionAsync(int optionId, UpdateOptionClientRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/options/{optionId}", request);
    }

    public async Task<HttpResponseMessage> DeleteOptionAsync(int optionId)
    {
        return await _httpClient.DeleteAsync($"api/options/{optionId}");
    }

    public async Task<HttpResponseMessage> ArchiveOptionAsync(int optionId)
    {
        return await _httpClient.PostAsync($"api/options/{optionId}/archive", null);
    }

    public async Task<HttpResponseMessage> RestoreOptionAsync(int optionId)
    {
        return await _httpClient.PostAsync($"api/options/{optionId}/restore", null);
    }
}

// ─── Client DTOs ─────────────────────────────────────────────────

public class QuizSummaryResponse
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
    public int QuestionCount { get; set; }
}

public class QuizDetailClientResponse
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
    public List<QuestionClientResponse> Questions { get; set; } = new();
}

public class QuestionClientResponse
{
    public int QuestionId { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuestionOptionClientResponse> Options { get; set; } = new();
}

public class QuestionOptionClientResponse
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateQuizClientRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
}

public class UpdateQuizClientRequest
{
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
}

public class CreateQuestionClientRequest
{
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<CreateOptionClientRequest> Options { get; set; } = new();
}

public class UpdateQuestionClientRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class CreateOptionClientRequest
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class UpdateOptionClientRequest
{
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
