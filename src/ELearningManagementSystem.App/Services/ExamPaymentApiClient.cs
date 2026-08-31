using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ELearningManagementSystem.App.Services;

public class ExamPaymentApiClient
{
    private readonly HttpClient _httpClient;

    public ExamPaymentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StudentCourseExamStatusResponse>?> GetMyAvailableExamsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<StudentCourseExamStatusResponse>>("api/course-exams/my-available");
    }

    public async Task<StudentCourseExamStatusResponse?> GetMyExamPaymentStatusAsync(int examId)
    {
        return await _httpClient.GetFromJsonAsync<StudentCourseExamStatusResponse>($"api/course-exams/{examId}/my-payment");
    }

    public async Task<HttpResponseMessage> SubmitPaymentAsync(int examId, string paymentMethod, string transactionId, byte[] imageBytes, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(paymentMethod), "paymentMethod");
        content.Add(new StringContent(transactionId), "transactionId");

        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "screenshot", fileName);

        return await _httpClient.PostAsync($"api/course-exams/{examId}/payments", content);
    }

    public async Task<PagedResult<ExamPaymentResponse>?> GetAdminPaymentsAsync(int page = 1, int pageSize = 10, string status = "Pending", string? searchTerm = null)
    {
        var queryParams = $"?page={page}&pageSize={pageSize}&status={status}";
        if (!string.IsNullOrEmpty(searchTerm)) queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

        return await _httpClient.GetFromJsonAsync<PagedResult<ExamPaymentResponse>>($"api/admin/exam-payments{queryParams}");
    }

    public async Task<HttpResponseMessage> ReviewPaymentAsync(int paymentId, bool approve, string? rejectionReason = null)
    {
        var request = new ReviewExamPaymentRequest
        {
            Approve = approve,
            RejectionReason = rejectionReason
        };

        return await _httpClient.PostAsJsonAsync($"api/admin/exam-payments/{paymentId}/review", request);
    }
}

public class StudentCourseExamStatusResponse
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int PoolQuestionCount { get; set; }
    public string PaymentStatus { get; set; } = "NotPaid";
    public int? ActivePaymentId { get; set; }
    public int? ActiveAttemptId { get; set; }
    public string? RejectionReason { get; set; }
    public bool CanTakeExam { get; set; }
    public bool? LastAttemptPassed { get; set; }
}

public class ExamPaymentResponse
{
    public int ExamPaymentId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string ScreenshotUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public bool IsUsed { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewExamPaymentRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}
