using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        return await _httpClient.GetFromJsonAsync<List<PermissionResponse>>("api/permissions");
    }
}

public class PermissionResponse
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
