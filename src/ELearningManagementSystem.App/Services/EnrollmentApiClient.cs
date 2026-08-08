using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace ELearningManagementSystem.App.Services;

public class EnrollmentApiClient
{
    private readonly HttpClient _httpClient;

    public EnrollmentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, EnrollmentSummaryResponse? Data, string? Error)> EnrollAsync(int courseId)
    {
        var response = await _httpClient.PostAsync($"api/courses/{courseId}/enrollments", null);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<EnrollmentSummaryResponse>();
            return (true, data, null);
        }

        var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, null, errorObj?.Error ?? "An unknown error occurred.");
    }

    public async Task<IEnumerable<MyCourseResponse>> GetMyEnrollmentsAsync()
    {
        var response = await _httpClient.GetAsync("api/enrollments/my");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<MyCourseResponse>>() ?? Array.Empty<MyCourseResponse>();
        }
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Array.Empty<MyCourseResponse>();
        }

        throw new Exception("Failed to load enrollments.");
    }

    public async Task<bool> CheckEnrollmentStatusAsync(int courseId)
    {
        var response = await _httpClient.GetAsync($"api/courses/{courseId}/enrollment");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<EnrollmentStatusResponse>();
            return data?.IsEnrolled ?? false;
        }
        
        return false;
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }

    private class EnrollmentStatusResponse
    {
        public bool IsEnrolled { get; set; }
    }
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
}
