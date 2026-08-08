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
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
