using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class LessonApiClient
{
    private readonly HttpClient _httpClient;

    public LessonApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<LessonSummaryResponse>?> GetLessonsAsync(int courseId, bool? isArchived = false)
    {
        var url = $"api/courses/{courseId}/lessons";
        if (isArchived.HasValue)
        {
            url += $"?isArchived={isArchived.Value.ToString().ToLower()}";
        }
        return await _httpClient.GetFromJsonAsync<IEnumerable<LessonSummaryResponse>>(url);
    }

    public async Task<LessonDetailResponse?> GetLessonAsync(int courseId, int lessonId)
    {
        return await _httpClient.GetFromJsonAsync<LessonDetailResponse>($"api/courses/{courseId}/lessons/{lessonId}");
    }

    public async Task<HttpResponseMessage> CreateLessonAsync(int courseId, CreateLessonRequest request)
    {
        return await _httpClient.PostAsJsonAsync($"api/courses/{courseId}/lessons", request);
    }

    public async Task<HttpResponseMessage> UpdateLessonAsync(int courseId, int lessonId, UpdateLessonRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/courses/{courseId}/lessons/{lessonId}", request);
    }

    public async Task<HttpResponseMessage> ArchiveLessonAsync(int courseId, int lessonId)
    {
        return await _httpClient.PostAsync($"api/courses/{courseId}/lessons/{lessonId}/archive", null);
    }

    public async Task<HttpResponseMessage> RestoreLessonAsync(int courseId, int lessonId)
    {
        return await _httpClient.PostAsync($"api/courses/{courseId}/lessons/{lessonId}/restore", null);
    }
}

public class LessonSummaryResponse
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool DeleteFlag { get; set; }
}

public class LessonDetailResponse
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
}

public class CreateLessonRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class UpdateLessonRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
