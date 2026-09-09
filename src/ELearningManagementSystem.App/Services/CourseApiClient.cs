using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class CourseApiClient
{
    private readonly HttpClient _httpClient;

    public CourseApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<CourseSummaryResponse>?> GetCoursesAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isArchived = false, bool includeDeleted = false)
    {
        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (isArchived.HasValue) queryParams += $"&isArchived={isArchived.Value.ToString().ToLower()}";
        else if (includeDeleted) queryParams += "&includeDeleted=true";

        return await _httpClient.GetFromJsonAsync<PagedResult<CourseSummaryResponse>>($"api/courses{queryParams}");
    }

    public async Task<CourseDetailResponse?> GetCourseAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<CourseDetailResponse>($"api/courses/{id}");
    }

    public async Task<HttpResponseMessage> CreateCourseAsync(CreateCourseRequest request)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Title), "Title");
        if (request.Description != null)
            content.Add(new StringContent(request.Description), "Description");
        content.Add(new StringContent(request.CategoryId.ToString()), "CategoryId");

        if (request.ThumbnailBytes != null)
        {
            var fileContent = new ByteArrayContent(request.ThumbnailBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ThumbnailMimeType ?? "image/jpeg");
            content.Add(fileContent, "Thumbnail", request.ThumbnailFileName ?? "thumbnail.jpg");
        }

        return await _httpClient.PostAsync("api/courses", content);
    }

    public async Task<HttpResponseMessage> UpdateCourseAsync(int id, UpdateCourseRequest request)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Title), "Title");
        if (request.Description != null)
            content.Add(new StringContent(request.Description), "Description");
        content.Add(new StringContent(request.CategoryId.ToString()), "CategoryId");
        content.Add(new StringContent(request.RemoveThumbnail.ToString().ToLower()), "RemoveThumbnail");

        if (request.RowVersion != null)
            content.Add(new StringContent(request.RowVersion), "RowVersion");

        if (request.ThumbnailBytes != null)
        {
            var fileContent = new ByteArrayContent(request.ThumbnailBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ThumbnailMimeType ?? "image/jpeg");
            content.Add(fileContent, "Thumbnail", request.ThumbnailFileName ?? "thumbnail.jpg");
        }

        return await _httpClient.PutAsync($"api/courses/{id}", content);
    }

    public async Task<HttpResponseMessage> ArchiveCourseAsync(int id)
    {
        return await _httpClient.PostAsync($"api/courses/{id}/archive", null);
    }

    public async Task<HttpResponseMessage> RestoreCourseAsync(int id)
    {
        return await _httpClient.PostAsync($"api/courses/{id}/restore", null);
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
    public string CategoryName { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool DeleteFlag { get; set; }
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
    public bool DeleteFlag { get; set; }
    public string? RowVersion { get; set; }
}

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public byte[]? ThumbnailBytes { get; set; }
    public string? ThumbnailFileName { get; set; }
    public string? ThumbnailMimeType { get; set; }
}

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public byte[]? ThumbnailBytes { get; set; }
    public string? ThumbnailFileName { get; set; }
    public string? ThumbnailMimeType { get; set; }
    public bool RemoveThumbnail { get; set; }
    public string? RowVersion { get; set; }
}
