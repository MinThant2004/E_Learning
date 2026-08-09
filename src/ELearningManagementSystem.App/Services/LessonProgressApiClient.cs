using System.Net.Http.Json;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace ELearningManagementSystem.App.Services;

public class LessonProgressApiClient
{
    private readonly HttpClient _httpClient;

    public LessonProgressApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CourseProgressResponse?> GetCourseProgressAsync(int courseId)
    {
        return await _httpClient.GetFromJsonAsync<CourseProgressResponse>($"api/courses/{courseId}/progress");
    }

    public async Task<ApiResult<LessonProgressResponse>> CompleteLessonAsync(int courseId, int lessonId)
    {
        var response = await _httpClient.PostAsync($"api/courses/{courseId}/lessons/{lessonId}/complete", null);
        var result = new ApiResult<LessonProgressResponse>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode
        };

        if (response.IsSuccessStatusCode)
        {
            result.Value = await response.Content.ReadFromJsonAsync<LessonProgressResponse>();
        }
        else
        {
            result.ErrorMessage = await ApiResponseHelper.GetErrorMessageAsync(response);
        }
        return result;
    }
}

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Value { get; set; }
}


public class CourseProgressResponse
{
    public int CompletedCount { get; set; }
    public int TotalActiveLessons { get; set; }
    public double Percentage => TotalActiveLessons == 0 ? 0 : Math.Round((double)CompletedCount / TotalActiveLessons * 100, 2);
    public bool IsCourseComplete => TotalActiveLessons > 0 && CompletedCount >= TotalActiveLessons;
    
    public IEnumerable<LessonProgressResponse> Lessons { get; set; } = new List<LessonProgressResponse>();
}

public class LessonProgressResponse
{
    public int LessonId { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }
}
