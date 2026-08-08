using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class CategoryApiClient
{
    private readonly HttpClient _httpClient;

    public CategoryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<CategoryResponse>?> GetCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<CategoryResponse>>("api/categories");
    }

    public async Task<CategoryResponse?> GetCategoryAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<CategoryResponse>($"api/categories/{id}");
    }

    public async Task<HttpResponseMessage> CreateCategoryAsync(CreateCategoryRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/categories", request);
    }

    public async Task<HttpResponseMessage> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/categories/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
    {
        return await _httpClient.DeleteAsync($"api/categories/{id}");
    }
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
