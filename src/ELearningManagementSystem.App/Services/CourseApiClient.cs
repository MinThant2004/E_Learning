using System.Net.Http.Json;

namespace ELearningManagementSystem.App.Services;

public class CourseApiClient
{
    private readonly HttpClient _httpClient;

    public CourseApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<CourseSummaryResponse>?> GetCoursesAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool includeDeleted = false)
    {
        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (includeDeleted) queryParams += "&includeDeleted=true";

        return await _httpClient.GetFromJsonAsync<PagedResult<CourseSummaryResponse>>($"api/courses{queryParams}");
    }

    public async Task<CourseDetailResponse?> GetCourseAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<CourseDetailResponse>($"api/courses/{id}");
    }

    public async Task<HttpResponseMessage> CreateCourseAsync(CreateCourseRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/courses", request);
    }

    public async Task<HttpResponseMessage> UpdateCourseAsync(int id, UpdateCourseRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/courses/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteCourseAsync(int id)
    {
        return await _httpClient.DeleteAsync($"api/courses/{id}");
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

public class CourseSummaryResponse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public bool Status { get; set; }
}

public class CourseDetailResponse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}
