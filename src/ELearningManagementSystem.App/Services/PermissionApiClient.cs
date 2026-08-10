using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace ELearningManagementSystem.App.Services;

public class PermissionApiClient
{
    private readonly HttpClient _httpClient;

    public PermissionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PermissionResponse>?> GetAllPermissionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/permissions");
            if (!response.IsSuccessStatusCode)
            {
                throw new CustomApiException(response.StatusCode, $"API returned status code {response.StatusCode}");
            }
            return await response.Content.ReadFromJsonAsync<List<PermissionResponse>>();
        }
        catch (HttpRequestException ex)
        {
            throw new CustomApiException(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}

public class PermissionResponse
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
