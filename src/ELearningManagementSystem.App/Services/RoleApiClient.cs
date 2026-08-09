using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ELearningManagementSystem.App.Services;

public class RoleApiClient
{
    private readonly HttpClient _httpClient;

    public RoleApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<RoleResponse>?> GetRolesAsync(RoleListQuery query)
    {
        var queryString = $"?page={query.Page}&pageSize={query.PageSize}";
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryString += $"&searchTerm={Uri.EscapeDataString(query.SearchTerm)}";
        }
        return await _httpClient.GetFromJsonAsync<PagedResult<RoleResponse>>($"api/roles{queryString}");
    }

    public async Task<RoleDetailResponse?> GetRoleByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<RoleDetailResponse>($"api/roles/{id}");
    }

    public async Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/roles", request);
        if (response.IsSuccessStatusCode) return (true, null);
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<(bool Success, string? Error)> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/roles/{id}", request);
        if (response.IsSuccessStatusCode) return (true, null);
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<(bool Success, string? Error)> AssignPermissionsAsync(int id, AssignPermissionsRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/roles/{id}/permissions", request);
        if (response.IsSuccessStatusCode) return (true, null);
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }
}

public class RoleListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleDetailResponse : RoleResponse
{
    public List<int> PermissionIds { get; set; } = new();
}

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignPermissionsRequest
{
    public List<int> PermissionIds { get; set; } = new();
}
