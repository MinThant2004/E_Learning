using System.Net.Http.Json;
using ELearningManagementSystem.Application.Features.ExamEngine.DTOs;

namespace ELearningManagementSystem.App.Services;

public class ExamEngineApiClient
{
    private readonly HttpClient _httpClient;

    public ExamEngineApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExamAttemptSessionResponse?> StartExamAttemptAsync(int examId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/course-exams/{examId}/attempts/start", new { });
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception(content);
        }
        return await response.Content.ReadFromJsonAsync<ExamAttemptSessionResponse>();
    }

    public async Task<ExamAttemptSessionResponse?> GetActiveAttemptSessionAsync(int attemptId)
    {
        return await _httpClient.GetFromJsonAsync<ExamAttemptSessionResponse>($"api/course-exams/attempts/{attemptId}");
    }

    public async Task<bool> SaveAnswerAsync(int attemptId, SaveAnswerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/course-exams/attempts/{attemptId}/answers", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<ExamResultResponse?> SubmitExamAttemptAsync(int attemptId, SubmitExamAttemptRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/course-exams/attempts/{attemptId}/submit", request);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception(content);
        }
        return await response.Content.ReadFromJsonAsync<ExamResultResponse>();
    }

    public async Task<ExamResultResponse?> GetAttemptResultAsync(int attemptId)
    {
        return await _httpClient.GetFromJsonAsync<ExamResultResponse>($"api/course-exams/attempts/{attemptId}/result");
    }

    public async Task<PublicCertificateResponse?> GetPublicCertificateAsync(int attemptId)
    {
        return await _httpClient.GetFromJsonAsync<PublicCertificateResponse>($"api/course-exams/certificates/verify/{attemptId}");
    }

    public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadCertificateFileAsync(int attemptId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/course-exams/certificates/download/{attemptId}");
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                               ?? "EduSphere_Certificate.html";
                return (true, bytes, fileName, null);
            }

            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, null, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }

    public async Task<List<PublicCertificateResponse>> GetMyCertificatesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PublicCertificateResponse>>("api/course-exams/my-certificates");
        return result ?? new List<PublicCertificateResponse>();
    }
}
